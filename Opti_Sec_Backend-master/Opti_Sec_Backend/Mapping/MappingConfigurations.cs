using Mapster;
using Opti_Sec_Backend.Contracts.Clients;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Client, ClientResponse>()
        .Map(dest => dest.Email, src => src.User.Email)
        .Map(dest => dest.UserName, src => src.User.UserName)
        .Map(dest => dest.Name, src => $"{src.FName} {src.LName}")
        .Map(dest => dest.Id, src => src.Id)
        .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
        .Map(dest => dest.ImageUrl, src => src.ImageUrl);

       
    }
}
