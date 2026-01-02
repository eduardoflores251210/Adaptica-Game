using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Simple JSON index for microbe files. Keeps minimal metadata to avoid scanning all files on every lookup.
/// Stored at Paths.CellsIndex (if available) or "microbe_index.json" in application persistent data path.
/// </summary>
public class MicrobeIndex
{
	[Serializable]
	public class RevisionEntry
	{
		public string Path;
		public string Name;
		public int revission;
		public string AssociatedSave;
	}

	[Serializable]
	public class IndexData
	{
		public List<RevisionEntry> Entries = new List<RevisionEntry>();
	}

	private IndexData _data = new IndexData();
	private string _indexPath;

	public MicrobeIndex(string indexPath = null)
	{
		_indexPath = indexPath;
		if (string.IsNullOrEmpty(_indexPath))
			_indexPath = Path.Combine(Application.persistentDataPath, "microbe_index.json");

		LoadOrBuild();
	}

	private void LoadOrBuild()
	{
		try
		{
			if (File.Exists(_indexPath))
			{
				string json = File.ReadAllText(_indexPath);
				_data = JsonUtility.FromJson<IndexData>(json) ?? new IndexData();
			}
			else
			{
				RebuildIndex();
				Save();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"MicrobeIndex load failed: {ex.Message}. Rebuilding index.");
			RebuildIndex();
			Save();
		}
	}

	/// <summary>
	/// Rebuild the index scanning both main cells and backups.
	/// Assigns revission = 0 for main save files (Paths.Cells) and increments for backups (Paths.BackUPCells)
	/// based on chronological order so backups get sequential revision numbers.
	/// </summary>
	public void RebuildIndex()
	{
		_data = new IndexData();
		try
		{
			var allFiles = new List<(string Path, DateTime Time, bool IsBackup)>();

			if (Directory.Exists(Paths.Cells))
			{
				foreach (var f in Directory.EnumerateFiles(Paths.Cells, "*.json", SearchOption.TopDirectoryOnly))
				{
					allFiles.Add((f, File.GetLastWriteTimeUtc(f), false));
				}
			}

			if (Directory.Exists(Paths.BackUPCells))
			{
				foreach (var f in Directory.EnumerateFiles(Paths.BackUPCells, "*.json", SearchOption.TopDirectoryOnly))
				{
					allFiles.Add((f, File.GetLastWriteTimeUtc(f), true));
				}
			}

			// sort by modification time ascending so revisions increase chronologically
			var sorted = allFiles.OrderBy(x => x.Time).ToList();

			// counters per microbe name: last assigned revision (0 means main save seen / default)
			var revisionCounters = new Dictionary<string, int>(StringComparer.Ordinal);

			foreach (var item in sorted)
			{
				try
				{
					var txt = File.ReadAllText(item.Path);
					var m = JsonUtility.FromJson<SerializableTypes.Biology.MicrobeData>(txt);
					if (m == null) continue;

					revisionCounters.TryGetValue(m.Name, out int lastRev);

					int assigned = item.IsBackup ? lastRev + 1 : 0;

					var entry = new RevisionEntry
					{
						Path = item.Path,
						Name = m.Name,
						revission = assigned,
						AssociatedSave = null
					};

					_data.Entries.Add(entry);

					// update counter: backups increase counter, main save ensures counter exists at least 0
					if (item.IsBackup)
						revisionCounters[m.Name] = assigned;
					else
						revisionCounters[m.Name] = Math.Max(lastRev, 0);
				}
				catch
				{
					continue;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"MicrobeIndex rebuild failed: {ex.Message}");
		}
	}

	public void Save()
	{
		try
		{
			string tmp = _indexPath + ".tmp";
			var json = JsonUtility.ToJson(_data);
			File.WriteAllText(tmp, json);
			if (File.Exists(_indexPath)) File.Delete(_indexPath);
			File.Move(tmp, _indexPath);
		}
		catch (Exception ex)
		{
			Debug.LogError($"MicrobeIndex save failed: {ex.Message}");
		}
	}

	/// <summary>
	/// Add or update an entry. Pass revision = 0 for main saves, >0 for backups.
	/// associatedSave can be used to link this revision to a save id if you use one.
	/// </summary>
	public void AddOrUpdateEntry(string path, string name, int revision = 0, string associatedSave = null)
	{
		try
		{
			var existing = _data.Entries.FirstOrDefault(e => string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase));
			if (existing != null)
			{
				existing.Name = name;
				existing.revission = revision;
				existing.AssociatedSave = associatedSave;
			}
			else
			{
				_data.Entries.Add(new RevisionEntry
				{
					Path = path,
					Name = name,
					revission = revision,
					AssociatedSave = associatedSave
				});
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"MicrobeIndex AddOrUpdateEntry failed: {ex.Message}");
		}
	}

	/// <summary>
	/// Get the path for the latest revision (largest revission number) for the given microbe name.
	/// </summary>
	public bool TryGetLatestRevisionPath(string name, out string path)
	{
		path = null;
		var matches = _data.Entries.Where(e => string.Equals(e.Name, name, StringComparison.Ordinal)).ToList();
		if (matches.Count == 0) return false;
		var best = matches.OrderBy(e => e.revission).Last();
		path = best.Path;
		return true;
	}

	/// <summary>
	/// Get the path for a specific revision number (revission). Returns false if not found.
	/// </summary>
	public bool TryGetRevisionPath(string name, int revisionNumber, out string path)
	{
		path = null;
		var match = _data.Entries.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.Ordinal) && e.revission == revisionNumber);
		if (match != null)
		{
			path = match.Path;
			return true;
		}

		// fallback: if caller used an index-style number (0..N-1), try that
		var list = _data.Entries.Where(e => string.Equals(e.Name, name, StringComparison.Ordinal)).OrderBy(e => e.revission).ToList();
		if (list.Count == 0) return false;
		if (revisionNumber >= 0 && revisionNumber < list.Count)
		{
			path = list[revisionNumber].Path;
			return true;
		}

		return false;
	}

	public bool TryGetAnyByName(string name, out string path)
	{
		path = null;
		var match = _data.Entries.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.Ordinal));
		if (match == null) return false;
		path = match.Path;
		return true;
	}

	/// <summary>
	/// Helper: next revision number to use for a backup (returns 1 if no backups yet).
	/// </summary>
	public int GetNextRevisionNumber(string name)
	{
		var matches = _data.Entries.Where(e => string.Equals(e.Name, name, StringComparison.Ordinal)).ToList();
		if (matches.Count == 0) return 1;
		return matches.Max(e => e.revission) + 1;
	}
}