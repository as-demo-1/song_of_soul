#if USE_CELTX
// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Celtx
{
    public class CeltxDataRaw
    {
        public string version;
        public CxDoc doc;
    }

    public class CxDoc
    {
        public string type;
        public List<CxContent> content;
    }

    public class CxContent
    {
        public string type;
        public CxAttrs attrs;
        public List<CxContent> content;
        
        // cxgameplay
        public string text;
        public List<CxContent> marks;
    }

    public class CxAttrs
    {
        public string id;
        public string catalog_id;

        public string branch_ref;
        public string name;
        public string desc;
        public bool first;
        public float node_x;
        public float node_y;
        public int level;
        public string shape;
        public string color;
        public bool dashes;
        public CxAAInstDescs a_a_inst_descs;
        public CxAssociatedAssetIds associated_asset_ids;
        public List<CxTransition> transitions;
        public string condition_ref;
        public CxLock lock_info;
        
        // cxinteractive_branch
        public string interactive_ref;
        
        // cxinteractive_branch, cxbranch, cxjump
        public string sequence_ref;
        public string sequence_color;

        // cxinteractive_branch, cxbranch
        public List<CxLinked> linked;

        // cxjump
        public CxLinked linked_jump;

        // cxbranch, cxjump
        public bool sequence_dashes;
        
        // cxinteractive_root, cxinteractive_sequence
        public string seqId;
        public string seqName;

        // cxbreakdown
        public string type;
        public string instance_desc;
        public string design_notes;
        public string dev_notes;
        public string media;

        // cxcharacter
        public bool isNew;

        // cxcomment
        public string commentId;
        public string commentColor;
        
        // cxcatalog_item
        public string title;
        public CxItemData item_data;
        public List<CxCustomVar> custom_vars;

        // cxconditions
        public List<CxVariable> variables;

        // cxcondition_item
        public List<CxLiteral> literals;
        public string clause;
        public List<CxOnObj> on;

        public string item_data_string;
        //public int N_ids;
        //public int N_names;
    }

    public class CxAAInstDescs
    {

    }

    public class CxAssociatedAssetIds
    {

    }

    public class CxTransition
    {
        public string id;
    }

    public class CxLinked
    {
        public CxLinked(string id)
        {
            this.id = id;
        }
        
        public string id;
        public string name;
        public string desc;
        public string color;
    }

    public class CxItemData
    {
        public string description;
        public string design;
        public string dev;
        public List<CxMedia> media;
        public string animation;
        public string general;
        public string story;
        public string dev_status;

        // mechanic
        public string mechanic_type;
        public string mechanic_trigger;

        // item
        public string item_type;
        public string item_properties;
        public string item_availability;

        // environmental
        public string environmental_type;

        // event
        public string event_trigger;

        // event & mechanic
        public string trigger_description;

        // location
        public string interior;
        public string exterior;

        // character
        public string character_type;
        public string biography;

        // sequence
        public string objectives;
    }

    public class CxLock
    {
        public bool isLocked;
        public string lockedBy;
        public string timestamp;
    }

    public class CxMedia
    {
        public string name;
        public string thumbPath;
        public string sourcePath;
        public string file_id;
        public string selfPath;
    }

    public class CxVariable
    {
        public string id;
        public string name;
        public string desc;
        public string type;
        public CxConfig config;
        public CxPermittedTypes permittedTypes;
        public string timestamp;
    }

    public class CxCustomVar
    {
        public string id;
        public string type;
        public CxConfig config;
        public bool boolVal;
        public long longVal;
        public string strVal;
    }

    public class CxConfig
    {
        public bool defaultBool; 
        public long defaultNum; // number, range
        public string defaultString; // date(yyyy-mm-dd), radio, textarea, text, time(hh:mm)

        // Radio var
        public List<CxOption> options;

        // Range var
        public long min;
        public long max;
    }

    public class CxOption
    {
        public string strVal;
        public string label;
    }

    public class CxPermittedTypes
    {
        public bool cxbranch;
        public bool cxsequence;
        public bool cxinteractive_branch;
        public bool cxjump;
    }

    public class CxLiteral
    {
        public string id;
        public string lit_name;
        public string name;
        public string type;
        public string varType;
        public CxConfig config;

        // variable
        public string comparisonOperator;

        // asset
        public string assetId;
        public string predicate;

        // condition
        public string conditionId;
        
        // variable & condition
        public CxComparisonValue comparisonValue;

        public string followingOp;
    }

    public class CxComparisonValue
    {
        public string type;
        public string strVal;
        public long longVal;
    }


    public class CxOnObj
    {
        public string from;
        public string to;
    }
}
#endif