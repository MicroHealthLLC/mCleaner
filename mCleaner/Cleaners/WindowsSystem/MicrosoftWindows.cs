using System.Collections.Generic;
using System.IO;
using mCleaner.Model;
using mCleaner.Properties;

namespace mCleaner.Cleaners
{
    public static class MicrosoftWindows
    {
        public static option AddCustomLocationsToTTD()
        {
            option o = new option()
            {
                id = "custom_locations",
                label = "Custom Location",
                description = "Delete user-specified files and folders.\r\nTo set it up, click Edit > Preferences > Custom Locations",
                level = 1,
                action = new List<action>()
            };

            if (Settings.Default.CustomLocationForDeletion != null)
            {
                foreach (string filepath in Settings.Default.CustomLocationForDeletion)
                {
                    if (File.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "delete",
                            search = "file",
                            path = filepath,
                            parent_option = o
                        });
                    }
                    else if (Directory.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "delete",
                            search = "walk.all",
                            path = filepath,
                            parent_option = o
                        });
                    }
                }
            }

            return o;
        }

        public static option AddClamAVCustomLocationsToTTD()
        {
            option o = new option()
            {
                id = "clamav_custom_locations",
                label = "ClamAV Custom Location",
                description = "Scan user-specified files or folder.\r\nTo set it up, click Edit > Preferences > Clam Anti Virus Tab > Scan Locations Tab",
                action = new List<action>()
            };

            if (Settings.Default.ClamWin_ScanLocations != null)
            {
                foreach (string filepath in Settings.Default.ClamWin_ScanLocations)
                {
                    if (File.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "clamscan",
                            search = "clamscan.file",
                            path = filepath,
                            parent_option = o
                        });
                    }
                    else if (Directory.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "clamscan",
                            search = "clamscan.folder",
                            path = filepath,
                            parent_option = o
                        });
                    }
                }
            }

            return o;
        }

        public static option AddClipboardCleaner()
        {
            option o = new option()
            {
                id = "clipboard",
                label = "Clipboard",
                description = "The desktop environment's clipboard used for copy and paste operations",
                level = 1,
                action = new List<action>()
            };

            o.action.Add(new action()
            {
                command = "clipboard",
                search = "clipboard.clear",
                parent_option = o
            });

            return o;
        }

        public static option AddWindowsLogsCleaner()
        {
            option o = new option()
            {
                id = "windows_logs",
                label = "Windows Logs",
                description = "Delete the logs",
                level = 1,
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$ALLUSERSPROFILE\\Application Data\\Microsoft\\Dr Watson\\*.log",
                "$ALLUSERSPROFILE\\Application Data\\Microsoft\\Dr Watson\\user.dmp",
                "$LocalAppData\\Microsoft\\Windows\\WER\\ReportArchive\\*\\*",
                "$LocalAppData\\Microsoft\\Windows\\WER\\ReportQueue\\*\\*",
                "$programdata\\Microsoft\\Windows\\WER\\ReportArchive\\*\\*",
                "$programdata\\Microsoft\\Windows\\WER\\ReportQueue\\*\\*",
                "$localappdata\\Microsoft\\Internet Explorer\\brndlog.bak",
                "$localappdata\\Microsoft\\Internet Explorer\\brndlog.txt",
                "$windir\\*.log",
                "$windir\\imsins.BAK",
                "$windir\\OEWABLog.txt",
                "$windir\\SchedLgU.txt",
                "$windir\\ntbtlog.txt",
                "$windir\\setuplog.txt",
                "$windir\\REGLOCS.OLD",
                "$windir\\Debug\\*.log",
                "$windir\\Debug\\Setup\\UpdSh.log",
                "$windir\\Debug\\UserMode\\*.log",
                "$windir\\Debug\\UserMode\\ChkAcc.bak",
                "$windir\\Debug\\UserMode\\userenv.bak",
                "$windir\\Microsoft.NET\\Framework\\*\\*.log",
                "$windir\\pchealth\\helpctr\\Logs\\hcupdate.log",
                "$windir\\security\\logs\\*.log",
                "$windir\\security\\logs\\*.old",
                "$windir\\system32\\TZLog.log",
                "$windir\\system32\\config\\systemprofile\\Application Data\\Microsoft\\Internet Explorer\\brndlog.bak",
                "$windir\\system32\\config\\systemprofile\\Application Data\\Microsoft\\Internet Explorer\\brndlog.txt",
                "$windir\\system32\\LogFiles\\AIT\\AitEventLog.etl.???",
                "$windir\\system32\\LogFiles\\Firewall\\pfirewall.log*",
                "$windir\\system32\\LogFiles\\Scm\\SCM.EVM*",
                "$windir\\system32\\LogFiles\\WMI\\Terminal*.etl",
                "$windir\\system32\\LogFiles\\WMI\\RTBackup\\EtwRT.*etl",
                "$windir\\system32\\wbem\\Logs\\*.lo_",
                "$windir\\system32\\wbem\\Logs\\*.log"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "glob",
                    path = path,
                    parent_option = o
                });
            }

            return o;
        }

        public static option AddTemporaryFilesCleaner()
        {
            option o = new option()
            {
                id = "windows_temp_files",
                label = "Temporary Files",
                description = "Delete the temporary files",
                level = 1,
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$USERPROFILE\\Local Settings\\Temp\\",
                "$windir\\temp\\"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.all",
                    path = path,
                    parent_option = o
                });
            }

            return o;
        }

        public static option AddMemoryDumpCleaner()
        {
            option o = new option()
            {
                id = "windows_memory_dump",
                label = "Memory Dump",
                description = "Delete the file memory.dmp",
                level = 1,
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$windir\\memory.dmp",
                "$windir\\Minidump\\*.dmp"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.files",
                    path = path,
                    parent_option = o
                });
            }

            return o;
        }

        #region DeepScan cleaners
        public static option AddDeepScan_Backup_Cleaner()
        {
            option o = new option()
            {
                id = "windows_deepscan_backup",
                label = "Backup Files",
                description = "It will scan the entire system drive and look for .bak files",
                warning = "This option will be very slow.",
                level = 1,
                action = new List<action>()
            };

            o.action.Add(new action()
            {
                command = "delete",
                search = "walk.all",
                path = "C:\\",
                regex = "\\.[Bb][Aa][Kk]$",
            });

            return o;
        }

        public static option AddDeepScan_ThumbsDB_Cleaner()
        {
            option o = new option()
            {
                id = "windows_deepscan_thumbsdb",
                label = "Thumbs.db Files",
                description = "It will scan the entire system drive and look for Thumbs.db files",
                warning = "This option will be very slow.",
                level = 2,
                action = new List<action>()
            };

            o.action.Add(new action()
            {
                command = "delete",
                search = "walk.all",
                path = "C:\\",
                regex = "Thumbs\\.db"
            });

            return o;
        }

        public static option AddDeepScan_OfficeTemp_Cleaner()
        {
            option o = new option()
            {
                id = "windows_deepscan_tempsfile",
                label = "Office Temporary Files",
                description = "It will scan the entire system drive and look for Microsoft Office temporary files",
                warning = "This option will be very slow.",
                level = 1,
                action = new List<action>()
            };

            // http://support.microsoft.com/kb/211632
            o.action.Add(new action()
            {
                command = "delete",
                search = "walk.all",
                path = "C:\\",
                regex = "~wr[a-z][0-9]{4}\\.tmp$"
            });

            // http://support.microsoft.com/kb/826810
            o.action.Add(new action()
            {
                command = "delete",
                search = "walk.all",
                path = "C:\\",
                regex = "ppt[0-9]{4}\\.tmp"
            });

            return o;
        }
        #endregion

        public static option AddMUICacheCleaner()
        {
            option o = new option()
            {
                id = "windows_muicache",
                label = "MUICache",
                description = "Delete the cache",
                level = 2,
                action = new List<action>()
            };

            string[] paths = new string[] {
                "HKCU\\Software\\Microsoft\\Windows\\ShellNoRoam\\MUICache",
                "HKCU\\Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "winreg",
                    path = path,
                    parent_option = o
                });
            }

            return o;
        }

        public static option AddPrefetchCleaner()
        {
            option o = new option()
            {
                id = "windows_prefetch",
                label = "Prefetch",
                description = "Delete the cache",
                level = 2,
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$windir\\Prefetch\\*.pf"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "glob",
                    path = path,
                    parent_option = o
                });
            }

            return o;
        }

        public static option AddUpdateUninstallersCleaner()
        {
            option o = new option()
            {
                id = "windows_update_uninstallers",
                label = "Update uninstallers",
                description = "Delete uninstallers for Microsoft updates including hotfixes, service packs, and Internet Explorer updates",
                level = 3,
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$windir\\SoftwareDistribution\\Download",
                "$windir\\ie7updates",
                "$windir\\ie8updates",
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.files",
                    path = path,
                    parent_option = o
                });
            }

            return o;
        }

        public static option AddRecycleBinCleaner()
        {
            option o = new option()
            {
                id = "windows_recyclebin",
                label = "Recycle bin",
                description = "Empty the recycle bin",
                level = 1,
                action = new List<action>()
            };

            var drvs = DriveInfo.GetDrives();
            List<string> drivenames = new List<string>();
            foreach (var drv in drvs)
            {
                if (drv.DriveType == DriveType.Fixed)
                {
                    drivenames.Add(drv.Name);
                }
            }

            List<string> paths = new List<string>();

            foreach (string drive in drivenames)
            {
                paths.Add(Path.Combine(drive, "$RECYCLE.BIN"));
            }

            //string[] paths = new string[] {
            //    "$windir\\SoftwareDistribution\\Download",
            //    "$windir\\ie7updates",
            //    "$windir\\ie8updates",
            //};

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.files",
                    path = path,
                    parent_option = o
                });
            }

            return o;
        }

        //public static option AddWindowsExplorerCleaner()
        //{

        //}
    }
}
