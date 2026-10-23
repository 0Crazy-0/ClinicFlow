using ClinicFlow.Application.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.CheckPatientSsnLast4;

public sealed class CheckPatientSsnLast4QueryHandler(
    ISsnHasher ssnHasher,
    IPatientRepository patientRepository
) : IRequestHandler<CheckPatientSsnLast4Query, bool>
{
    public async Task<bool> Handle(
        CheckPatientSsnLast4Query request,
        CancellationToken cancellationToken
    )
    {
        var ssnLast4Hash = ssnHasher.ComputeHash(request.PatientId, request.Last4);

        return await patientRepository.ExistsWithSsnLast4HashAsync(
            request.PatientId,
            ssnLast4Hash,
            cancellationToken
        );
    }
}
