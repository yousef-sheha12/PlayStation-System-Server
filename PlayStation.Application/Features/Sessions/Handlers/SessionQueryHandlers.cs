using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Session;
using PlayStation.Application.Features.Sessions.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Domain.Enums;

namespace PlayStation.Application.Features.Sessions.Handlers;

public class GetAllSessionsHandler : IRequestHandler<GetAllSessionsQuery, Result<List<SessionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSessionsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<SessionDto>>> Handle(GetAllSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Repository<Session>().FindAsync(s => !s.IsDeleted);
        var sessionDtos = _mapper.Map<List<SessionDto>>(sessions);
        return Result<List<SessionDto>>.Success(sessionDtos);
    }
}

public class GetSessionByIdHandler : IRequestHandler<GetSessionByIdQuery, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SessionDto>> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(request.Id);
        if (session == null || session.IsDeleted)
            return Result<SessionDto>.Failure("Session not found");

        var sessionDto = _mapper.Map<SessionDto>(session);
        return Result<SessionDto>.Success(sessionDto);
    }
}

public class GetActiveSessionsHandler : IRequestHandler<GetActiveSessionsQuery, Result<List<SessionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetActiveSessionsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<SessionDto>>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Repository<Session>().FindAsync(s =>
            !s.IsDeleted && (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));
        var sessionDtos = _mapper.Map<List<SessionDto>>(sessions);
        return Result<List<SessionDto>>.Success(sessionDtos);
    }
}

public class GetSessionsByStatusHandler : IRequestHandler<GetSessionsByStatusQuery, Result<List<SessionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionsByStatusHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<SessionDto>>> Handle(GetSessionsByStatusQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Repository<Session>().FindAsync(s => !s.IsDeleted && s.Status == request.Status);
        var sessionDtos = _mapper.Map<List<SessionDto>>(sessions);
        return Result<List<SessionDto>>.Success(sessionDtos);
    }
}

public class GetSessionsByDeviceHandler : IRequestHandler<GetSessionsByDeviceQuery, Result<List<SessionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionsByDeviceHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<SessionDto>>> Handle(GetSessionsByDeviceQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Repository<Session>().FindAsync(s => !s.IsDeleted && s.DeviceId == request.DeviceId);
        var sessionDtos = _mapper.Map<List<SessionDto>>(sessions);
        return Result<List<SessionDto>>.Success(sessionDtos);
    }
}

public class GetSessionsByDateRangeHandler : IRequestHandler<GetSessionsByDateRangeQuery, Result<List<SessionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionsByDateRangeHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<SessionDto>>> Handle(GetSessionsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Repository<Session>().FindAsync(s =>
            !s.IsDeleted &&
            s.StartTime >= request.StartDate &&
            s.StartTime <= request.EndDate.AddDays(1));
        var sessionDtos = _mapper.Map<List<SessionDto>>(sessions);
        return Result<List<SessionDto>>.Success(sessionDtos);
    }
}
