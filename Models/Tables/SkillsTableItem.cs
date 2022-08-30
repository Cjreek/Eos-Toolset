﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class SkillsTableItem : TableItem
    {
        public Skill? Skill { get; set; }
        public bool IsClassSkill { get; set; }
    }
}