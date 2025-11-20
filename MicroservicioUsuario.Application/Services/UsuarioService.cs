using MicroservicioUsuario.Domain.Entities;
using MicroservicioUsuario.Domain.Interfaces;
using MicroservicioUsuario.Domain.Security; // ✅ CLASE DE HASHING
using System.Collections.Generic;
using System.Linq;
using System;

namespace MicroservicioUsuario.Application.Services
{
    public class UsuarioService
    {
        private readonly IRepository<Usuario> _repo;

        public UsuarioService(IRepository<Usuario> repo)
        {
            _repo = repo;
        }

        // ==========================
        // CRUD
        // ==========================

        public IEnumerable<Usuario> Listar()
        {
            return _repo.Listar().Where(u => u.Estado == 1);
        }

        public Usuario? Obtener(int id) => _repo.Obtener(id);

        public void Crear(Usuario u)
        {
            // ✅ HASHEAR CONTRASEÑA ANTES DE GUARDAR
            u.Contraseña = PasswordHasher.HashPassword(u.Contraseña);
            u.RequiereCambioContraseña = true; // Por defecto, requiere cambio
            _repo.Crear(u);
        }

        public void Editar(Usuario u)
        {
            var usuarioExistente = _repo.Obtener(u.Id_Usuario);
            if (usuarioExistente == null)
                throw new ArgumentException("Usuario no encontrado");

            // ✅ SOLO HASHEAR SI LA CONTRASEÑA CAMBIÓ
            if (u.Contraseña != usuarioExistente.Contraseña)
            {
                u.Contraseña = PasswordHasher.HashPassword(u.Contraseña);
            }

            _repo.Editar(u);
        }

        public void Eliminar(int id) => _repo.Eliminar(id);

        // ==========================
        // LOGIN (Con PBKDF2 + Soporte para hashes antiguos)
        // ==========================

        public Usuario? Login(string emailOrUser, string password)
        {
            var usuarios = _repo.Listar().ToList();

            var usuario = usuarios.FirstOrDefault(u =>
                (!string.IsNullOrEmpty(u.Email) &&
                 u.Email.Equals(emailOrUser, StringComparison.OrdinalIgnoreCase))
                ||
                (!string.IsNullOrEmpty(u.NombreUsuario) &&
                 u.NombreUsuario.Equals(emailOrUser, StringComparison.OrdinalIgnoreCase))
            );

            if (usuario == null)
                return null;

            // ✅ SISTEMA DUAL: Detectar si es hash nuevo o viejo
            bool passwordValida = false;

            // Verificar si el hash en BD fue generado por nuestro código
            if (PasswordHasher.IsValidHashFormat(usuario.Contraseña))
            {
                // Hash nuevo (generado por nuestro código)
                passwordValida = PasswordHasher.VerifyPassword(password, usuario.Contraseña);
            }
            else
            {
                // ⚠️ Hash viejo o contraseña en texto plano
                // OPCIÓN 1: Comparación directa (si la BD tiene texto plano temporalmente)
                passwordValida = (usuario.Contraseña == password);
                
                // Si la contraseña es correcta, migrar automáticamente al nuevo formato
                if (passwordValida)
                {
                    try
                    {
                        usuario.Contraseña = PasswordHasher.HashPassword(password);
                        _repo.Editar(usuario);
                        Console.WriteLine($"✅ Usuario {usuario.NombreUsuario} migrado a nuevo hash PBKDF2");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error al migrar usuario {usuario.NombreUsuario}: {ex.Message}");
                    }
                }
            }
  
            if (!passwordValida)
                return null;

            return usuario;
        }

        public bool CambiarContraseña(int id, string actual, string nueva)
        {
            var usuario = _repo.Obtener(id);
            if (usuario == null)
                return false;

            // ✅ VERIFICAR CONTRASEÑA ACTUAL (soporta ambos formatos)
            bool passwordActualValida = false;

            if (PasswordHasher.IsValidHashFormat(usuario.Contraseña))
            {
                // Hash nuevo
                passwordActualValida = PasswordHasher.VerifyPassword(actual, usuario.Contraseña);
            }
            else
            {
                // Hash viejo o texto plano
                passwordActualValida = (usuario.Contraseña == actual);
            }
  
            if (!passwordActualValida)
                return false;

            // ✅ HASHEAR NUEVA CONTRASEÑA
            usuario.Contraseña = PasswordHasher.HashPassword(nueva);
            usuario.RequiereCambioContraseña = false; // Ya no requiere cambio
            usuario.UltimaModificacion = DateTime.Now;

            _repo.Editar(usuario);

            return true;
        }
    }
}
