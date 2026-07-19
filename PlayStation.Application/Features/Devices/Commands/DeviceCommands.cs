using MediatR;
using PlayStation.Application.DTOs.Device;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Devices.Commands;

public record CreateDeviceCommand(CreateDeviceDto Device) : IRequest<Result<int>>;
public record UpdateDeviceCommand(int Id, UpdateDeviceDto Device) : IRequest<Result>;
public record DeleteDeviceCommand(int Id) : IRequest<Result>;
