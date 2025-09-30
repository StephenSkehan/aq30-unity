// Assets/Editor/AQ/Leads/LeadsPlaygroundWindow.cs
#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AQ.App.Leads;

namespace AQ.Editor.Leads
{
    public sealed class LeadsPlaygroundWindow : EditorWindow
    {
        private const string kDefaultDbPath = "Assets/App/Leads/Data/LeadsDatabase.asset";

        [SerializeField] private LeadsDatabase _database;

        [MenuItem("AQ/Leads/Playground Window", priority = 20)]
        public static void Open() => GetWindow<LeadsPlaygroundWindow>("Leads Playground");

        private void OnEnable()
        {
            if (_database == null)
                _database = AssetDatabase.LoadAssetAtPath<LeadsDatabase>(kDefaultDbPath);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Database", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                _database = (LeadsDatabase)EditorGUILayout.ObjectField(_database, typeof(LeadsDatabase), false);

                if (GUILayout.Button("Create / Locate Default DB", GUILayout.Width(200)))
                {
                    _database = CreateOrLocateDefaultDb();
                    EditorGUIUtility.PingObject(_database);
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Push All → Runtime", GUILayout.Height(24)))
                {
                    PushAllToRuntime(_database);
                }

                if (GUILayout.Button("Ping Repository In Scene", GUILayout.Height(24), GUILayout.Width(200)))
                {
                    var repo = FindRepo();
                    if (repo != null)
                    {
                        Selection.activeObject = repo.gameObject;
                        EditorGUIUtility.PingObject(repo);
                        Debug.Log("[AQ] LeadsPlayground: Repository found & pinged.", repo);
                    }
                    else
                    {
                        Debug.LogWarning("[AQ] LeadsPlayground: No LeadsRepository found in the open scene.");
                    }
                }
            }
        }

        private static LeadsDatabase CreateOrLocateDefaultDb()
        {
            var asset = AssetDatabase.LoadAssetAtPath<LeadsDatabase>(kDefaultDbPath);
            if (asset != null) return asset;

            var dir = Path.GetDirectoryName(kDefaultDbPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var db = ScriptableObject.CreateInstance<LeadsDatabase>();
            AssetDatabase.CreateAsset(db, kDefaultDbPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AQ] Created LeadsDatabase at '{kDefaultDbPath}'.", db);
            return db;
        }

        private static LeadsRepository FindRepo()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<LeadsRepository>();
#else
            // Fallback for older Unity versions
            return Object.FindObjectOfType<LeadsRepository>();
#endif
        }

        private static void PushAllToRuntime(LeadsDatabase db)
        {
            if (db == null)
            {
                Debug.LogWarning("[AQ] Push: No LeadsDatabase assigned.");
                return;
            }

            var repo = FindRepo();
            if (repo == null)
            {
                Debug.LogWarning("[AQ] Push: No LeadsRepository found in the open scene.");
                return;
            }

            // Assign property named "Database" if present
            var repoType = repo.GetType();
            var dbProp = repoType.GetProperty("Database", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (dbProp != null && dbProp.CanWrite)
            {
                dbProp.SetValue(repo, db);
                EditorUtility.SetDirty(repo);
            }

            // Call any refresh-like method if it exists
            var refresh =
                repoType.GetMethod("RefreshAll", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                repoType.GetMethod("RebuildFromDatabase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                repoType.GetMethod("Rebroadcast", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (refresh != null)
            {
                refresh.Invoke(repo, null);
            }

            Debug.Log("[AQ] Push: Database pushed to runtime repository.", repo);
        }
    }
}
#endif
