using Entities.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IAuthenticationManager
    {
        Task<bool> ValdiateUser(UserForAuthenticationDto userForAuth);
        Task<string> CreateToken();
    }
}
