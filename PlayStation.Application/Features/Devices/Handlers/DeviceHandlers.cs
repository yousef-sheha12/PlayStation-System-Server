using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Device;
using PlayStation.Application.Features.Devices.Commands;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Domain.Enums;

namespace PlayStation.Application.Features.Devices.Handlers;

public class CreateDeviceHandler : IRequestHandler<CreateDeviceCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDeviceHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = _mapper.Map<Device>(request.Device);
        device.Status = DeviceStatus.Available;
        await _unitOfWork.Repository<Device>().AddAsync(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(device.Id, "Device created successfully");
    }
}

public class UpdateDeviceHandler : IRequestHandler<UpdateDeviceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateDeviceHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Repository<Device>().GetByIdAsync(request.Id);
        if (device == null)
            return Result.Failure("Device not found");

        _mapper.Map(request.Device, device);
        device.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Device>().UpdateAsync(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Device updated successfully");
    }
}

public class DeleteDeviceHandler : IRequestHandler<DeleteDeviceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDeviceHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Repository<Device>().GetByIdAsync(request.Id);
        if (device == null)
            return Result.Failure("Device not found");

        var hasActiveSessions = (await _unitOfWork.Repository<Session>().FindAsync(s =>
            s.DeviceId == request.Id && (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused))).Any();

        if (hasActiveSessions)
            return Result.Failure("Cannot delete device with active sessions");

        device.IsDeleted = true;
        device.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Device>().UpdateAsync(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Device deleted successfully");
    }
}
