
using Consts;

using System.Collections.Generic;
using UnityEngine;

namespace LIFULSE.Manager
{
    public partial class GameInfoManager
    {
        // 메인스토리, 인연스토리, 이벤트 스토리 관리 메니저
        private Dictionary<CONTENTS_TYPE_ID, Dictionary<int, List<Stage_TableData >>> _storyDics = new Dictionary<CONTENTS_TYPE_ID, Dictionary<int, List<Stage_TableData>>>();

        public Dictionary<CONTENTS_TYPE_ID, Dictionary<int, List<Stage_TableData>>> StoryDics { get { return _storyDics; } }
        public List<CONTENTS_TYPE_ID> GetStoryDivisions()
        {
            List<CONTENTS_TYPE_ID> list = new List<CONTENTS_TYPE_ID>(); 
            foreach (var data in _storyDics)
            {
                list.Add(data.Key);
            }

            return list;
        }

        public string GetStroyGroupName(CONTENTS_TYPE_ID typeID)
        {
            string str = string.Empty;
            switch (typeID)
            {
                case CONTENTS_TYPE_ID.story_main:
                    str = "str_ui_chapter_story_title";// 메인 스토리
                    break;
                case CONTENTS_TYPE_ID.story_event:
                    str = "str_library_tap_event"; // 이벤트 스토리
                    break;
            }
            return str;
        }
    }

}
