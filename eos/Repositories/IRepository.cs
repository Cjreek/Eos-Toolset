﻿using Eos.Models;
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
        BaseModel? GetBaseByIndex(int index);
        int GetNextFreeIndex(int startIndex = 0);
        int? GetBase2DAIndex(BaseModel model, bool returnCustomDataIndex = true);
        IEnumerable<BaseModel?> GetItems();
    }
}
