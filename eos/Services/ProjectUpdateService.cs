using Eos.Config;
using Eos.Repositories;
using Eos.Updates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Services
{
    public class ProjectUpdateService
    {
        private EosProject _project;
        private int _nextUpdateIndex = -1;
        private bool _needsGameDataUpdate;

        private static List<Update> updateList = new List<Update>()
        {
            new Update0630(),
            new Update0640(),
            new Update0760(),
        };

        public bool NeedsGameDataUpdate => _needsGameDataUpdate;

        public ProjectUpdateService(EosProject project)
        {
            _project = project;
        }

        public bool CheckForUpdates()
        {
            _needsGameDataUpdate = false;
            _nextUpdateIndex = updateList.Count;
            var indexDone = false;
            for (int i = 0; i < updateList.Count; i++)
            {
                if (updateList[i].Version > _project.Version)
                {
                    if (EosConfig.BaseGameDataBuildDate < updateList[i].GameDataMinimumBuildDate)
                    {
                        if (updateList[i].GameDataMinimumBuildDate > EosConfig.CurrentGameBuildDate) // Needs higher version than available
                            break;
                        
                        _needsGameDataUpdate = true;
                    }

                    if (!indexDone)
                    {
                        _nextUpdateIndex = i;
                        indexDone = true;
                    }

                    if ((indexDone) && (NeedsGameDataUpdate))
                        break;
                }
            }

            return _nextUpdateIndex < updateList.Count;
        }

        public bool ApplyUpdates()
        {
            if (_nextUpdateIndex < 0) // CheckForUpdates first!
                return false;

            for (int i = _nextUpdateIndex; i < updateList.Count; i++)
            {
                if (EosConfig.CurrentGameBuildDate < updateList[i].GameDataMinimumBuildDate)
                    break;

                updateList[i].Apply(_project);
                _project.Version = updateList[i].Version;
            }

            return true;
        }

        public static int GetHighestSupportedVersion()
        {
            var result = 0;
            for (int i = updateList.Count - 1; i >= 0; i--)
            {
                if (EosConfig.BaseGameDataBuildDate < updateList[i].GameDataMinimumBuildDate)
                    break;

                result = updateList[i].Version;
            }

            return result;
        }
    }
}
