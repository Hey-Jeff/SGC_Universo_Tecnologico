using Microsoft.AspNetCore.Identity;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new Usuario(), password);

    public bool Verificar(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        return _hasher.VerifyHashedPassword(new Usuario(), hash, password) != PasswordVerificationResult.Failed;
    }
}
