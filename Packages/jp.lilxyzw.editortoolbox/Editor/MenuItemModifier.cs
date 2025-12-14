using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal static class MenuItemModifier
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.delayCall -= ReplaceMenuItems;
            EditorApplication.delayCall += ReplaceMenuItems;
            EditorToolboxSettingsEditor.update -= ReplaceMenuItems;
            EditorToolboxSettingsEditor.update += ReplaceMenuItems;
        }

        private static bool isChanged = false;

        private static void ReplaceMenuItems()
        {
            if(!isChanged && !EditorToolboxSettings.instance.enableMenuDirectoryReplaces) return;
            ReplaceMenuItems(EditorToolboxSettings.instance.menuDirectoryReplaces);
        }

        private static void GetNameAndShortcut(string menuItem, out string name, out string shortcut)
        {
            var ind = menuItem.IndexOfAny("_%#^".ToCharArray());
            if(ind == -1 || menuItem[ind-1] != ' ')
            {
                name = menuItem;
                shortcut = "";
            }
            else
            {
                name = menuItem[..(ind-1)];
                shortcut = menuItem[ind..];
            }
        }

        private static void ReplaceMenuItems(MenuReplace[] replaces)
        {
            MenuWrap.RebuildAllMenus();
            WindowLayoutWrap.UpdateWindowLayoutMenu();
            isChanged = false;
            if(!EditorToolboxSettings.instance.enableMenuDirectoryReplaces) return;

            isChanged = true;
            foreach(var replace in replaces)
            {
                if(!replace.from.EndsWith("*"))
                {
                    ReplaceMenuItemDirectory(replace.from, replace.to);
                    continue;
                }
                var from = replace.from[..^1];
                foreach(var r in menuItems.Where(m => m.Key.StartsWith(from)))
                    ReplaceMenuItemDirectory(r.Key, string.IsNullOrEmpty(replace.to) ? "" : r.Key.Replace(from, replace.to));
            }

            if(replaces.Length > 0 && replaces.All(r => string.IsNullOrEmpty(r.to)))
            {
                MenuWrap.AddMenuItem("Help/Temp", "", false, 1000, () => {}, null);
            }
            EditorUtilityWrap.Internal_UpdateAllMenus();
        }

        private static void ReplaceMenuItemDirectory(string from, string to)
        {
            if(!origins.TryGetValue(from, out var menuItem))
            {
                if(menuItems.TryGetValue(from, out var mis))
                {
                    from = MenuServiceWrap.SanitizeMenuItemName(from);
                    GetNameAndShortcut(from, out string name, out string shortcut);
                    menuItem = new(){
                        menuItem = name,
                        shortcut = shortcut,
                        priority = mis.First().Item2.priority
                    };
                    var fncs = mis.Select(m => m.Item1);
                    var exct = mis.Where(m => !m.Item2.validate && m.Item1.ReturnType == typeof(void) && MenuServiceWrap.ValidateMethodForMenuCommand(m.Item1)).Select(m => m.Item1).FirstOrDefault();
                    var vld = mis.Where(m => m.Item2.validate && m.Item1.ReturnType == typeof(bool) && MenuServiceWrap.ValidateMethodForMenuCommand(m.Item1)).Select(m => m.Item1).FirstOrDefault();
                    if(exct != null)
                    {
                        var parameters = exct.GetParameters();
                        if(parameters.Length == 0)
                        {
                            menuItem.execute = Expression.Lambda<Action>(Expression.Call(null, exct)).Compile();
                        }
                        else if(parameters.Length == 1 && parameters[0].ParameterType == typeof(MenuCommand))
                        {
                            menuItem.isEnabled = false;
                            //var act = Expression.Lambda<Action<MenuCommand>>(Expression.Call(null, exct, Expression.Parameter(typeof(MenuItem), "command"))).Compile();
                            //menuItem.execute = () => act.Invoke(new MenuCommand(null));
                        }
                    }
                    if(vld != null)
                    {
                        menuItem.validate = Expression.Lambda<Func<bool>>(Expression.Call(null, vld)).Compile();
                    }
                    origins[from] = menuItem;
                }
                else
                {
                    menuItem = new(){
                        isEnabled = false
                    };
                }
            }

            if(string.IsNullOrEmpty(to))
            {
                MenuWrap.RemoveMenuItem(from);
            }
            else
            {
                if(!menuItem.isEnabled) return; // 不明なメニューは置き換えも削除もしない
                if(!to.Contains("/")) to = $"Root/{to}";
                GetNameAndShortcut(to, out string name, out string shortcut);
                MenuWrap.AddMenuItem(name, shortcut, Menu.GetChecked(from), menuItem.priority, menuItem.execute, menuItem.validate);
                MenuWrap.RemoveMenuItem(from);
            }
        }

        private static readonly Dictionary<string, MenuItemInternal> origins = new();
        private static readonly Dictionary<string, (MethodInfo, MenuItem)[]> menuItems = 
            TypeCache.GetMethodsWithAttribute<MenuItem>()
            .Select(m => (m,m.GetCustomAttributes<MenuItem>()))
            .Select(m => (m.m, m.Item2.First()))
            .GroupBy(m => m.Item2.menuItem).ToDictionary(m => m.Key, m => m.ToArray());

        private class MenuItemInternal
        {
            public string menuItem;
            public string shortcut;
            public int priority;
            public Action execute;
            public Func<bool> validate;
            public bool isEnabled = true;
        }
    }

    [Serializable]
    internal class MenuReplace : DirectElements
    {
        public string from = "";
        public string to = "";
    }
}
