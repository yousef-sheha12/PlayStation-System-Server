using MediatR;
using PlayStation.Application.DTOs.Device;
using PlayStation.Domain.Common;
using PlayStation.Domain.Enums;

namespace PlayStation.Application.Features.Devices.Queries;

public record GetAllDevicesQuery() : IRequest<Result<List<DeviceDto>>>;
public record GetDeviceByIdQuery(int Id) : IRequest<Result<DeviceDto>>;
public record GetDevicesByStatusQuery(DeviceStatus Status) : IRequest<Result<List<DeviceDto>>>;
