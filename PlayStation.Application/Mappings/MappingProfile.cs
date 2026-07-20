using AutoMapper;
using PlayStation.Application.DTOs.Product;
using PlayStation.Application.DTOs.Category;
using PlayStation.Application.DTOs.Device;
using PlayStation.Application.DTOs.Customer;
using PlayStation.Application.DTOs.Expense;
using PlayStation.Application.DTOs.Session;
using PlayStation.Application.DTOs.Invoice;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.Quantity <= src.LowStockThreshold && src.Quantity > 0))
            .ForMember(dest => dest.IsOutOfStock, opt => opt.MapFrom(src => src.Quantity == 0));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));

        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        CreateMap<Device, DeviceDto>();
        CreateMap<CreateDeviceDto, Device>();
        CreateMap<UpdateDeviceDto, Device>();

        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.SessionCount, opt => opt.MapFrom(src => src.Sessions.Count));

        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();

        CreateMap<Expense, ExpenseDto>();
        CreateMap<CreateExpenseDto, Expense>();
        CreateMap<UpdateExpenseDto, Expense>();

        CreateMap<Session, SessionDto>()
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.Device.Name))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : src.CustomerName))
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.SessionProducts))
            .ForMember(dest => dest.SessionProducts, opt => opt.MapFrom(src => src.SessionProducts));

        CreateMap<SessionProduct, SessionProductDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.Session.Device.Name))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Session.Customer != null ? src.Session.Customer.Name : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.InvoiceItems));

        CreateMap<InvoiceItem, InvoiceItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

        CreateMap<Product, ProductStockAlertDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
    }
}
