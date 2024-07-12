using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using Penumbra.Import.Structs;
using Penumbra.UI.Classes;

namespace Penumbra.Import;

public partial class TexToolsImporter
{
    // Progress Data
    private int _currentModPackIdx;
    private int _currentOptionIdx;
    private int _currentFileIdx;

    private int    _currentNumOptions;
    private int    _currentNumFiles;
    private string _currentModName    = string.Empty;
    private string _currentGroupName  = string.Empty;
    private string _currentOptionName = string.Empty;
    private string _currentFileName   = string.Empty;

    public bool DrawProgressInfo(Vector2 size)
    {
        if (_modPackCount == 0)
        {
            ImGuiUtil.Center("Nothing to extract.");
            return false;
        }

        if (_modPackCount == _currentModPackIdx)
            return DrawEndState();

        ImGui.NewLine();
        var percentage = (float)_currentModPackIdx / _modPackCount;
        ImGui.ProgressBar(percentage, size, $"Mod {_currentModPackIdx + 1} / {_modPackCount}");
        ImGui.NewLine();
        if (State == ImporterState.DeduplicatingFiles)
            ImGui.TextUnformatted($"Deduplicating {_currentModName}...");
        else
            ImGui.TextUnformatted($"Extracting {_currentModName}...");

        if (_currentNumOptions > 1)
        {
            ImGui.NewLine();
            ImGui.NewLine();
            percentage = _currentNumOptions == 0 ? 1f : _currentOptionIdx / (float)_currentNumOptions;
            ImGui.ProgressBar(percentage, size, $"Option {_currentOptionIdx + 1} / {_currentNumOptions}");
            ImGui.NewLine();
            if (State != ImporterState.DeduplicatingFiles)
                ImGui.TextUnformatted(
                    $"Extracting option {(_currentGroupName.Length == 0 ? string.Empty : $"{_currentGroupName} - ")}{_currentOptionName}...");
        }

        ImGui.NewLine();
        ImGui.NewLine();
        percentage = _currentNumFiles == 0 ? 1f : _currentFileIdx / (float)_currentNumFiles;
        ImGui.ProgressBar(percentage, size, $"File {_currentFileIdx + 1} / {_currentNumFiles}");
        ImGui.NewLine();
        if (State != ImporterState.DeduplicatingFiles)
            ImGui.TextUnformatted($"Extracting file {_currentFileName}...");
        return false;
    }


    private bool DrawEndState()
    {
        var success = ExtractedMods.Count(t => t.Error == null);

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            return true;

        ImGui.TextUnformatted($"Successfully extracted {success} / {ExtractedMods.Count} files.");
        ImGui.NewLine();
        using var table = ImRaii.Table("##files", 2);
        if (!table)
            return false;

        foreach (var (file, dir, ex) in ExtractedMods)
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(file.Name);
            ImGui.TableNextColumn();
            if (ex == null)
            {
                using var color = ImRaii.PushColor(ImGuiCol.Text, ColorId.FolderExpanded.Value());
                ImGui.TextUnformatted(dir?.FullName[(_baseDirectory.FullName.Length + 1)..] ?? "Unknown Directory");
            }
            else
            {
                using var color = ImRaii.PushColor(ImGuiCol.Text, ColorId.ConflictingMod.Value());
                ImGui.TextUnformatted(ex.Message);
                ImGuiUtil.HoverTooltip(ex.ToString());
            }
        }

        return false;
    }

    public bool DrawCancelButton(Vector2 size)
        => ImGuiUtil.DrawDisabledButton("Cancel", size, string.Empty, _token.IsCancellationRequested);
}
