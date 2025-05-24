using App.Modelos;
using Microsoft.AspNetCore.Identity;

namespace App.Validators
{
    public class ValidarPassword : IPasswordValidator<Usuario>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<Usuario> manager, Usuario user, string? password)
        {
            var errores = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errores.Add(new IdentityError { Description = "La contraseña no puede estar vacía." });
            }
            else
            {
                if (password.Length < 6)
                    errores.Add(new IdentityError { Description = "La contraseña debe contener al menos 6 caracteres." });
                
                if (!password.Any(char.IsDigit))
                    errores.Add(new IdentityError { Description = "La contraseña debe contener al menos un número." });

                if (!password.Any(char.IsLower))
                    errores.Add(new IdentityError { Description = "La contraseña debe contener al menos una letra minúscula." });

                if (!password.Any(char.IsUpper))
                    errores.Add(new IdentityError { Description = "La contraseña debe contener al menos una letra mayúscula." });

                if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                    errores.Add(new IdentityError { Description = "La contraseña debe contener al menos un carácter especial." });
            }

            if (errores.Any())
                return await Task.FromResult(IdentityResult.Failed(errores.ToArray()));

            return await Task.FromResult(IdentityResult.Success);
        }
    }
}
