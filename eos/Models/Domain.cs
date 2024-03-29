﻿using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class Domain : BaseModel
    {
        private Spell? _level0Spell;
        private Spell? _level1Spell;
        private Spell? _level2Spell;
        private Spell? _level3Spell;
        private Spell? _level4Spell;
        private Spell? _level5Spell;
        private Spell? _level6Spell;
        private Spell? _level7Spell;
        private Spell? _level8Spell;
        private Spell? _level9Spell;
        private Feat? _grantedFeat;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();

        public Spell? Level0Spell
        {
            get { return _level0Spell; }
            set { Set(ref _level0Spell, value); }
        }

        public Spell? Level1Spell
        {
            get { return _level1Spell; }
            set { Set(ref _level1Spell, value); }
        }

        public Spell? Level2Spell
        {
            get { return _level2Spell; }
            set { Set(ref _level2Spell, value); }
        }

        public Spell? Level3Spell
        {
            get { return _level3Spell; }
            set { Set(ref _level3Spell, value); }
        }

        public Spell? Level4Spell
        {
            get { return _level4Spell; }
            set { Set(ref _level4Spell, value); }
        }

        public Spell? Level5Spell
        {
            get { return _level5Spell; }
            set { Set(ref _level5Spell, value); }
        }

        public Spell? Level6Spell
        {
            get { return _level6Spell; }
            set { Set(ref _level6Spell, value); }
        }

        public Spell? Level7Spell
        {
            get { return _level7Spell; }
            set { Set(ref _level7Spell, value); }
        }

        public Spell? Level8Spell
        {
            get { return _level8Spell; }
            set { Set(ref _level8Spell, value); }
        }

        public Spell? Level9Spell
        {
            get { return _level9Spell; }
            set { Set(ref _level9Spell, value); }
        }

        public Feat? GrantedFeat
        {
            get { return _grantedFeat; }
            set { Set(ref _grantedFeat, value); }
        }

        public bool FeatIsActive { get; set; } = false;

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Domain?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Domain";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Domain";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            Level0Spell = Resolve(Level0Spell, MasterRepository.Spells);
            Level1Spell = Resolve(Level1Spell, MasterRepository.Spells);
            Level2Spell = Resolve(Level2Spell, MasterRepository.Spells);
            Level3Spell = Resolve(Level3Spell, MasterRepository.Spells);
            Level4Spell = Resolve(Level4Spell, MasterRepository.Spells);
            Level5Spell = Resolve(Level5Spell, MasterRepository.Spells);
            Level6Spell = Resolve(Level6Spell, MasterRepository.Spells);
            Level7Spell = Resolve(Level7Spell, MasterRepository.Spells);
            Level8Spell = Resolve(Level8Spell, MasterRepository.Spells);
            Level9Spell = Resolve(Level9Spell, MasterRepository.Spells);
            GrantedFeat = Resolve(GrantedFeat, MasterRepository.Feats);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
            this.Level0Spell = CreateRefFromJson<Spell>(json["Level0Spell"]?.AsObject());
            this.Level1Spell = CreateRefFromJson<Spell>(json["Level1Spell"]?.AsObject());
            this.Level2Spell = CreateRefFromJson<Spell>(json["Level2Spell"]?.AsObject());
            this.Level3Spell = CreateRefFromJson<Spell>(json["Level3Spell"]?.AsObject());
            this.Level4Spell = CreateRefFromJson<Spell>(json["Level4Spell"]?.AsObject());
            this.Level5Spell = CreateRefFromJson<Spell>(json["Level5Spell"]?.AsObject());
            this.Level6Spell = CreateRefFromJson<Spell>(json["Level6Spell"]?.AsObject());
            this.Level7Spell = CreateRefFromJson<Spell>(json["Level7Spell"]?.AsObject());
            this.Level8Spell = CreateRefFromJson<Spell>(json["Level8Spell"]?.AsObject());
            this.Level9Spell = CreateRefFromJson<Spell>(json["Level9Spell"]?.AsObject());
            this.GrantedFeat = CreateRefFromJson<Feat>(json["GrantedFeat"]?.AsObject());
            this.FeatIsActive = json["FeatIsActive"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var domainJson = base.ToJson();
            domainJson.Add("Name", this.Name.ToJson());
            domainJson.Add("Description", this.Description.ToJson());
            domainJson.Add("Icon", this.Icon);
            domainJson.Add("Level0Spell", CreateJsonRef(this.Level0Spell));
            domainJson.Add("Level1Spell", CreateJsonRef(this.Level1Spell));
            domainJson.Add("Level2Spell", CreateJsonRef(this.Level2Spell));
            domainJson.Add("Level3Spell", CreateJsonRef(this.Level3Spell));
            domainJson.Add("Level4Spell", CreateJsonRef(this.Level4Spell));
            domainJson.Add("Level5Spell", CreateJsonRef(this.Level5Spell));
            domainJson.Add("Level6Spell", CreateJsonRef(this.Level6Spell));
            domainJson.Add("Level7Spell", CreateJsonRef(this.Level7Spell));
            domainJson.Add("Level8Spell", CreateJsonRef(this.Level8Spell));
            domainJson.Add("Level9Spell", CreateJsonRef(this.Level9Spell));
            domainJson.Add("GrantedFeat", CreateJsonRef(this.GrantedFeat));
            domainJson.Add("FeatIsActive", this.FeatIsActive);

            return domainJson;
        }
    }
}
