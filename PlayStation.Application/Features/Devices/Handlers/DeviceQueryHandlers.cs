using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Device;
using PlayStation.Application.Features.Devices.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Domain.Enums;

namespace PlayStation.Application.Features.Devices.Handlers;

public class GetAllDevicesHandler : IRequestHandler<GetAllDevicesQuery, Result<List<DeviceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDevicesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<DeviceDto>>> Handle(GetAllDevicesQuery request, CancellationToken cancellationToken)
    {
        var devices = await _unitOfWork.Repository<Device>().FindAsync(d => !d.IsDeleted);
        var deviceDtos = _mapper.Map<List<DeviceDto>>(devices);

        foreach (var deviceDto in deviceDtos)
        {
            deviceDto.ActiveSessionCount = await _unitOfWork.Repository<Session>().CountAsync(s =>
                s.DeviceId == deviceDto.Id && !s.IsDeleted &&
                (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));
        }

        return Result<List<DeviceDto>>.Success(deviceDtos);
    }
}

public class GetDeviceByIdHandler : IRequestHandler<GetDeviceByIdQuery, Result<DeviceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDeviceByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DeviceDto>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Repository<Device>().GetByIdAsync(request.Id);
        if (device == null || device.IsDeleted)
            return Result<DeviceDto>.Failure("Device not found");

        var deviceDto = _mapper.Map<DeviceDto>(device);

        deviceDto.ActiveSessionCount = await _unitOfWork.Repository<Session>().CountAsync(s =>
            s.DeviceId == deviceDto.Id && !s.IsDeleted &&
            (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));

        return Result<DeviceDto>.Success(deviceDto);
    }
}

public class GetDevicesByStatusHandler : IRequestHandler<GetDevicesByStatusQuery, Result<List<DeviceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDevicesByStatusHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<DeviceDto>>> Handle(GetDevicesByStatusQuery request, CancellationToken cancellationToken)
    {
        var devices = await _unitOfWork.Repository<Device>().FindAsync(d => !d.IsDeleted && d.Status == request.Status);
        var deviceDtos = _mapper.Map<List<DeviceDto>>(devices);

        foreach (var deviceDto in deviceDtos)
        {
            deviceDto.ActiveSessionCount = await _unitOfWork.Repository<Session>().CountAsync(s =>
                s.DeviceId == deviceDto.Id && !s.IsDeleted &&
                (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));
        }

        return Result<List<DeviceDto>>.Success(deviceDtos);
    }
}
