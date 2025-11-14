using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ServiceCommon.Infrastructure.Persistence.Data;
using ServiceUsuario.Domain.Entities;
using ServiceUsuario.Domain.Builders;

namespace ServiceUsuario.Application.Service
{
    public class UsuarioService
    {
        private readonly MySqlRepositoryFactory<Usuario> _usuarioFactory;

        public UsuarioService(MySqlRepositoryFactory<Usuario> usuarioFactory)
        {
            _usuarioFactory = usuarioFactory;
        }

        public IEnumerable<Usuario> ObtenerTodosLosUsuarios()
        {
            var repo = _usuarioFactory.CreateRepository();
            return repo.GetAllAsync();
        }

        public Usuario ObtenerUsuarioPorId(int id)
        {
            var repo = _usuarioFactory.CreateRepository();
            return repo.GetByIdAsync(id);
        }

        public string CrearNuevoUsuarioConBuilder(string nombres, string primerApellido, string? segundoApellido, string email, string rol)
        {
            var (usuario, contraseñaPlana) = new UsuarioBuilder()
                .ConNombres(nombres)
                .ConApellidos(primerApellido, segundoApellido)
                .ConEmail(email)
                .ConRol(rol)
                .ConContraseñaAutomatica()
                .Construir();

            var repo = _usuarioFactory.CreateRepository();
            repo.AddAsync(usuario);

            return contraseñaPlana;
        }

        public string CrearNuevoUsuario(Usuario usuario)
        {
            var repo = _usuarioFactory.CreateRepository();
            if (!string.IsNullOrEmpty(usuario.Rol)) usuario.Rol = usuario.Rol.Trim();
            
            usuario.NombreUsuario = GenerarNombreUsuario(usuario.Email);
            
            string contraseñaGenerada = GenerarContraseñaAutomatica(usuario.Nombres, usuario.PrimerApellido);
            
            usuario.Contraseña = HashPassword(contraseñaGenerada);

            repo.AddAsync(usuario);
            
            return contraseñaGenerada;
        }

        public void ActualizarUsuario(Usuario usuario)
        {
            var repo = _usuarioFactory.CreateRepository();
            if (!string.IsNullOrEmpty(usuario.Rol)) usuario.Rol = usuario.Rol.Trim();
            if (!string.IsNullOrEmpty(usuario.Contraseña) && !usuario.Contraseña.StartsWith("PBKDF2:", StringComparison.Ordinal))
            {
                usuario.Contraseña = HashPassword(usuario.Contraseña);
            }

            repo.UpdateAsync(usuario);
        }

        public bool CambiarContraseña(int usuarioId, string contraseñaActual, string nuevaContraseña)
        {
            var repo = _usuarioFactory.CreateRepository();
            var usuario = repo.GetByIdAsync(usuarioId);

            if (usuario == null) return false;

            if (!VerifyHashedPassword(usuario.Contraseña, contraseñaActual))
            {
                return false;
            }

            string nuevaContraseñaHash = HashPassword(nuevaContraseña);
            var usuarioRepo = repo as Infrastructure.Persistence.Repositories.UsuarioRepository;
            usuarioRepo?.UpdatePassword(usuarioId, nuevaContraseñaHash);

            return true;
        }

        public void EliminarUsuario(int id)
        {
            var repo = _usuarioFactory.CreateRepository();
            repo.DeleteAsync(id);
        }

        public Usuario ValidarUsuario(string emailOrUsername, string password)
        {
            // ✅ NUEVO: Validar y limpiar input
            if (string.IsNullOrWhiteSpace(emailOrUsername) || string.IsNullOrWhiteSpace(password))
                return null;

            emailOrUsername = emailOrUsername.Trim();

            var repo = _usuarioFactory.CreateRepository();
            var usuarioRepo = repo as Infrastructure.Persistence.Repositories.UsuarioRepository;
            
            var usuario = usuarioRepo?.GetByEmailOrUsername(emailOrUsername);

            if (usuario == null) return null;

            if (!string.IsNullOrEmpty(usuario.Rol)) usuario.Rol = usuario.Rol.Trim();
            
            if (!string.IsNullOrEmpty(usuario.Contraseña) && usuario.Contraseña.StartsWith("PBKDF2:", StringComparison.Ordinal))
   {
     if (VerifyHashedPassword(usuario.Contraseña, password))
      {
       return usuario;
 }
          return null;
       }

    if (usuario.Contraseña == password)
            {
usuario.Contraseña = HashPassword(password);
     repo.UpdateAsync(usuario);
       return usuario;
      }

   return null;
        }

        private string GenerarNombreUsuario(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;

            var parteLocal = email.Split('@')[0];
            
            var nombreUsuario = Regex.Replace(parteLocal, @"[^a-zA-Z0-9._-]", "").ToLowerInvariant();

            return nombreUsuario;
        }

        private string GenerarContraseñaAutomatica(string nombres, string primerApellido)
        {
            string nombreLimpio = LimpiarTexto(nombres);
            string apellidoLimpio = LimpiarTexto(primerApellido);

            var primerNombre = nombreLimpio.Split(' ').FirstOrDefault() ?? nombreLimpio;
            string partNombre = primerNombre.Length >= 3 
                ? primerNombre.Substring(0, 3) 
                : primerNombre.PadRight(3, 'x');

            string partApellido = apellidoLimpio.Length >= 2 
                ? apellidoLimpio.Substring(0, 2) 
                : apellidoLimpio.PadRight(2, 'x');

            string contraseñaBase = char.ToUpper(partNombre[0]) + 
                                   partNombre.Substring(1).ToLower() + 
                                   partApellido.ToLower();

            string contraseñaFinal = contraseñaBase;
            int contador = 1;
            
            var repo = _usuarioFactory.CreateRepository();
            var todosUsuarios = repo.GetAllAsync().ToList();

            while (ContraseñaExiste(contraseñaFinal + "!", todosUsuarios))
            {
                contraseñaFinal = contraseñaBase + contador;
                contador++;
            }

            contraseñaFinal += "!";

            return contraseñaFinal;
        }

        private bool ContraseñaExiste(string contraseñaPlana, List<Usuario> usuarios)
        {
            foreach (var usuario in usuarios)
            {
                if (usuario.Contraseña.StartsWith("PBKDF2:", StringComparison.Ordinal))
                {
                    if (VerifyHashedPassword(usuario.Contraseña, contraseñaPlana))
                    {
                        return true;
                    }
                }
                else if (usuario.Contraseña == contraseñaPlana)
                {
                    return true;
                }
            }
            return false;
        }

        private string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            string normalizado = texto.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalizado)
            {
                if (char.IsLetter(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return $"PBKDF2:100000:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        private bool VerifyHashedPassword(string storedHash, string providedPassword)
        {
            try
            {
                var parts = storedHash.Split(':');
                if (parts.Length != 4) return false;
                if (parts[0] != "PBKDF2") return false;
                int iterations = int.Parse(parts[1]);
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] hash = Convert.FromBase64String(parts[3]);

                using var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, iterations, HashAlgorithmName.SHA256);
                byte[] testHash = pbkdf2.GetBytes(hash.Length);

                return CryptographicOperations.FixedTimeEquals(testHash, hash);
            }
            catch
            {
                return false;
            }
        }

        public bool EmailYaExiste(string email)
        {
     var repo = _usuarioFactory.CreateRepository();
          var usuarioExistente = repo.GetAllAsync()
     .FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
      return usuarioExistente != null;
      }
    }
}