using System.Text.RegularExpressions;

namespace MicroservicioUsuario.Domain.Validators
{
    /// <summary>
    /// Validador centralizado para prevenir inyecciones y validar datos de entrada
    /// </summary>
    public static class InputValidator
    {
     // Patrones peligrosos de SQL Injection
        private static readonly string[] SqlInjectionPatterns = new[]
        {
            @"(\bOR\b|\bAND\b).*=.*",
      @"';|--;|\/\*|\*\/",
    @"\bEXEC\b|\bEXECUTE\b",
      @"\bDROP\b|\bDELETE\b|\bUPDATE\b|\bINSERT\b",
   @"\bSELECT\b.*\bFROM\b",
            @"\bUNION\b.*\bSELECT\b",
     @"xp_cmdshell",
         @"\bSCRIPT\b.*>",
 @"<\s*script",
            @"javascript:",
      @"onerror\s*=",
            @"onload\s*="
        };

        /// <summary>
        /// Limpia y valida una cadena de texto general
   /// </summary>
        public static string SanitizeString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
     return string.Empty;

            // Trim de espacios
          input = input.Trim();

      // Normalizar espacios multiples
            input = Regex.Replace(input, @"\s+", " ");

   return input;
        }

        /// <summary>
        /// Valida y limpia nombres (permite letras, espacios, acentos, guiones)
        /// </summary>
        public static string SanitizeName(string? input)
     {
            if (string.IsNullOrWhiteSpace(input))
          return string.Empty;

      input = SanitizeString(input);

     // Solo permite letras (incluyendo acentos), espacios, guiones y apostrofes
            if (!Regex.IsMatch(input, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-']+$"))
      {
     throw new ArgumentException($"El nombre '{input}' contiene caracteres no permitidos.");
     }

            return input;
    }

    /// <summary>
        /// Valida email
        /// </summary>
        public static string SanitizeEmail(string? input)
        {
         if (string.IsNullOrWhiteSpace(input))
      return string.Empty;

            input = input.Trim().ToLower();

       // Patron RFC 5322 simplificado
         var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

      if (!Regex.IsMatch(input, emailPattern))
            {
                throw new ArgumentException($"El email '{input}' no es valido.");
       }

    return input;
        }

    /// <summary>
        /// Valida username (solo letras, numeros, guion bajo y punto)
    /// </summary>
public static string SanitizeUsername(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
    return string.Empty;

   input = input.Trim().ToLower();

 // Solo alfanumerico, guion bajo y punto
            if (!Regex.IsMatch(input, @"^[a-z0-9._]{3,50}$"))
            {
         throw new ArgumentException("El nombre de usuario debe tener entre 3-50 caracteres y solo puede contener letras, numeros, puntos y guiones bajos.");
 }

         return input;
        }

        /// <summary>
        /// Detecta patrones de SQL Injection
        /// </summary>
    public static bool ContainsSqlInjection(string? input)
      {
            if (string.IsNullOrWhiteSpace(input))
     return false;

  input = input.ToUpper();

    foreach (var pattern in SqlInjectionPatterns)
       {
           if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
        return true;
            }

          return false;
        }

        /// <summary>
        /// Valida que no contenga SQL Injection y limpia la entrada
/// </summary>
public static string ValidateAndSanitize(string? input, string fieldName)
        {
  if (string.IsNullOrWhiteSpace(input))
         return string.Empty;

  input = SanitizeString(input);

       if (ContainsSqlInjection(input))
            {
         throw new ArgumentException($"El campo '{fieldName}' contiene caracteres o patrones no permitidos.");
    }

     return input;
        }

        /// <summary>
      /// Valida texto largo (descripcion, comentarios) sin caracteres peligrosos
        /// </summary>
        public static string SanitizeText(string? input)
        {
     if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = SanitizeString(input);

  // Remover caracteres peligrosos para XSS
   input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
         input = Regex.Replace(input, @"javascript:", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"on\w+\s*=", "", RegexOptions.IgnoreCase);

     if (ContainsSqlInjection(input))
        {
 throw new ArgumentException("El texto contiene patrones no permitidos.");
            }

            return input;
     }

        /// <summary>
    /// Valida fecha (no puede ser anterior a hoy ni muy futura)
        /// </summary>
        public static DateTime ValidateDate(DateTime date, bool canBePast = false, bool canBeFuture = true)
    {
          var today = DateTime.Now.Date;
    var maxFutureDate = today.AddYears(10);

     if (!canBePast && date.Date < today)
   {
    throw new ArgumentException("La fecha no puede ser anterior a hoy.");
      }

            if (!canBeFuture && date.Date > today)
          {
          throw new ArgumentException("La fecha no puede ser futura.");
   }

      if (date > maxFutureDate)
     {
          throw new ArgumentException("La fecha esta demasiado lejos en el futuro (maximo 10 años).");
          }

            if (date.Year < 1900)
         {
            throw new ArgumentException("La fecha no es valida.");
     }

            return date;
        }

        /// <summary>
     /// Valida rango de fechas
        /// </summary>
        public static void ValidateDateRange(DateTime? startDate, DateTime? endDate)
        {
 if (startDate.HasValue && endDate.HasValue)
            {
         if (endDate.Value.Date < startDate.Value.Date)
                {
                throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
            }

           if (endDate.Value.Date == startDate.Value.Date)
        {
          throw new ArgumentException("La fecha de fin no puede ser igual a la fecha de inicio.");
}
            }
        }

        /// <summary>
     /// Valida contraseña (minimo 8 caracteres, mayuscula, minuscula, numero)
      /// </summary>
        public static void ValidatePassword(string? password)
        {
    if (string.IsNullOrWhiteSpace(password))
   {
    throw new ArgumentException("La contraseña no puede estar vacia.");
      }

       if (password.Length < 8)
     {
     throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
    }

     if (!Regex.IsMatch(password, @"[A-Z]"))
      {
                throw new ArgumentException("La contraseña debe contener al menos una mayuscula.");
            }

       if (!Regex.IsMatch(password, @"[a-z]"))
            {
     throw new ArgumentException("La contraseña debe contener al menos una minuscula.");
   }

  if (!Regex.IsMatch(password, @"[0-9]"))
            {
      throw new ArgumentException("La contraseña debe contener al menos un numero.");
            }
        }

        /// <summary>
        /// Valida rol
        /// </summary>
        public static string ValidateRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
     {
       throw new ArgumentException("El rol no puede estar vacio.");
   }

       role = role.Trim();

            var validRoles = new[] { "SuperAdmin", "JefeDeProyecto", "Empleado" };

            if (!validRoles.Contains(role))
            {
                throw new ArgumentException($"El rol '{role}' no es valido. Roles permitidos: {string.Join(", ", validRoles)}");
            }

   return role;
        }
    }
}
