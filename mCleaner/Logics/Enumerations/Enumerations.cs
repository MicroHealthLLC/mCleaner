using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBureau;

namespace mCleaner.Logics.Enumerations
{
    public enum COMMANDS
    {
        /// <summary>
        /// wala ...
        /// </summary>
        none, 

        /// <summary>
        /// delete any matching files from SEARCH
        /// 
        /// supported search:
        ///     file
        ///     glob
        ///     walk.files
        ///     walk.all
        ///     deep
        /// </summary>
        [StringValue("delete")]
        delete,

        /// <summary>
        /// defragments an SQLite 3 database
        /// 
        /// supported search:
        ///     file
        ///     glob
        ///     walk.files
        ///     walk.all
        ///     deep
        /// </summary>
        [StringValue("sqlite.vacuum")]
        sqlite_vacuum,

        /// <summary>
        /// LINUX COMMAND
        /// </summary>
        [StringValue("truncate")]
        truncate,

        /// <summary>
        /// delete a key or name in Windows Registry
        /// 
        /// supported search: none
        /// </summary>
        [StringValue("winreg")]
        winreg,

        // special commands

        #region chrome special commands
        [StringValue("chrome.databases_db")]
        chrome_database_db,

        [StringValue("chrome.autofill")]
        chrome_autofill,

        [StringValue("chrome.history")]
        chrome_history,

        [StringValue("chrome.favicons")]
        chrome_favicons,

        [StringValue("chrome.keywords")]
        chrome_keywords,
        #endregion

        [StringValue("json")]
        json,

        [StringValue("ini")]
        ini,

        [StringValue("clamscan")]
        clamscan,

        [StringValue("littleregistry")]
        littleregistry,

        [StringValue("clipboard")]
        clipboard,

        [StringValue("dupchecker")]
        dupchecker,
    }

    public enum SEARCH
    {
        /// <summary>
        /// wala ...
        /// </summary>
        none, 

        /// <summary>
        /// finds a single file
        /// glob does not work here. It should be straight path
        /// </summary>
        [StringValue("file")]
        file,

        /// <summary>
        /// look up all pathnames matching the shell glob pattern described in the Python documentation.
        /// doc: https://docs.python.org/2/library/glob.html
        /// </summary>
        [StringValue("glob")]
        glob,

        /// <summary>
        /// deletes all files (but not any directories), walking in to each subdirectory.
        /// SHOULD WORK ONLY FOR FOLDERS. 
        /// glob search works here as well
        /// </summary>
        [StringValue("walk.files")]
        walk_files,

        /// <summary>
        /// deletes all files and directories under the directory (but not the parent). It walks each subdirectory too.
        /// SHOULD WORK ONLY FOR FOLDERS. 
        /// glob search works here as well
        /// </summary>
        [StringValue("walk.all")]
        walk_all,

        /// <summary>
        /// 
        /// </summary>
        [StringValue("deep")]
        deep,

        // special commands made for mCleaner
        [StringValue("winreg.delete_entries")]
        winreg_delete_entries,

        #region ClamScan
        [StringValue("clamscan.folder")]
        clamscan_folder,

        [StringValue("clamscan.folder.recurse")]
        clamscan_folder_recurse,

        [StringValue("clamscan.memory")]
        clamscan_memory,

        [StringValue("clamscan.file")]
        clamscan_file,
        #endregion

        #region littl registry cleaner
        [StringValue("lrc.activexcom")]
        lrc_activex_com,

        [StringValue("lrc.appinfo")]
        lrc_app_info,

        [StringValue("lrc.progloc")]
        lrc_progam_location,

        [StringValue("lrc.software.settings")]
        lrc_software_settings,

        [StringValue("lrc.startup")]
        lrc_startup,

        [StringValue("lrc.sysdrv")]
        lrc_system_drivers,

        [StringValue("lrc.shareddll")]
        lrc_shared_dll,

        [StringValue("lrc.helpfile")]
        lrc_help_file,

        [StringValue("lrc.soundevent")]
        lrc_sound_event,

        [StringValue("lrc.histlist")]
        lrc_history_list,

        [StringValue("lrc.winfonts")]
        lrc_win_fonts,
        #endregion

        [StringValue("clipboard.clear")]
        clipboard_clear,

        [StringValue("dupchecker.all")]
        dupchecker_all
    }

    public enum THINGS_TO_DELETE
    {
        file,
        folder,
        regex,
        registry_key,
        registry_name,
        clamwin,
        littlregistrycleaner,
        system
    }
}
