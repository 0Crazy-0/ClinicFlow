namespace ClinicFlow.Application.Interfaces;

public interface IPasswordResetTokenService
{
    Task<string> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
