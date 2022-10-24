using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories
{
    public interface IRepository
    {
        void AddBase(BaseModel model);
        void RemoveBase(BaseModel model);
        bool HasOverride(BaseModel model);
        BaseModel? GetOverride(BaseModel model);
        BaseModel? GetBaseByID(Guid id);
    }
}
