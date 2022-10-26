using AutoMapper;

using SharpC2.API.Response;

using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class CredentialMapping : Profile
{
    public CredentialMapping()
    {
        CreateMap<Credential, CredentialDao>();
        CreateMap<CredentialDao, Credential>();
        CreateMap<Credential, CredentialResponse>();
    }
}