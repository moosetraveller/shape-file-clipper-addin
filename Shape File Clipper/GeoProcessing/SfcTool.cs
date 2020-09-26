using System;
using System.IO;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace Geomo.ShapeFileClipper.GeoProcessing
{
    public class SfcTool // TODO rename and refactor?
    {
        private readonly string _clipExtentShapeFile;
        private readonly string _outputDirectory;
        private readonly string _postfix;
        private readonly string _backupFolderName;
        private readonly OverwriteMode _overwriteMode;
        private readonly string _targetReferenceSystem;
        private readonly CancelableProgressorSource _cancelHandler;

        public SfcTool(string clipExtentShapeFile, string outputDirectory, string postfix, string backupFolderName,
            OverwriteMode overwriteMode, CancelableProgressorSource cancelHandler)
        {
            this._clipExtentShapeFile = clipExtentShapeFile;
            this._outputDirectory = outputDirectory;
            this._postfix = postfix;
            this._backupFolderName = backupFolderName;
            this._overwriteMode = overwriteMode;
            this._cancelHandler = cancelHandler;
        }

        public SfcTool(string clipExtentShapeFile, string outputDirectory, string postfix, string targetReferenceSystem, string backupFolderName,
            OverwriteMode overwriteMode, CancelableProgressorSource cancelHandler) : this(clipExtentShapeFile, outputDirectory, postfix, backupFolderName, overwriteMode, cancelHandler)
        {
            this._targetReferenceSystem = targetReferenceSystem;
        }

        private async Task<bool> CopyShapeFile(string shapeFile, string targetPath)
        {

            var copyToolParams
                = await QueuedTask.Run(() => Geoprocessing.MakeValueArray(shapeFile, targetPath));

            var result = await Geoprocessing.ExecuteToolAsync("management.CopyFeatures", copyToolParams, null,
                _cancelHandler.Progressor, GPExecuteToolFlags.None | GPExecuteToolFlags.GPThread);

            return !result.IsFailed;
        }

        private async Task<bool> DoBackupFileIfExists(string shapeFilePath)
        {

            if (!File.Exists(shapeFilePath))
            {
                return true;
            }
            var backupDirectory = Path.Combine(Path.GetDirectoryName(shapeFilePath) ?? "", _backupFolderName);
            var backupOutputPath = Path.Combine(backupDirectory, Path.GetFileName(shapeFilePath));
            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            return await CopyShapeFile(shapeFilePath, backupOutputPath);

        }

        private async Task<bool> Project(string shapeFile, string outputPath)
        {
            var projectToolParams
                = await QueuedTask.Run(() => Geoprocessing.MakeValueArray(shapeFile, outputPath, _targetReferenceSystem));

            var result = await Geoprocessing.ExecuteToolAsync("management.Project", projectToolParams, null,
                _cancelHandler.Progressor, GPExecuteToolFlags.None | GPExecuteToolFlags.GPThread);

            return !result.IsFailed;
        }

        private async Task<bool> Clip(string shapeFile, string outputPath)
        {

            var clipToolParams
                = await QueuedTask.Run(() => Geoprocessing.MakeValueArray(shapeFile, _clipExtentShapeFile, outputPath));

            var result = await Geoprocessing.ExecuteToolAsync("analysis.Clip", clipToolParams, null,
                _cancelHandler.Progressor, GPExecuteToolFlags.None | GPExecuteToolFlags.GPThread);

            return !result.IsFailed;

        }

        public async Task<bool> Process(string layer)
        {

            var shapeFileName = $"{Path.GetFileNameWithoutExtension(layer)}{_postfix}.shp";
            var outputPath = Path.Combine(_outputDirectory, shapeFileName);

            switch (_overwriteMode)
            {
                case OverwriteMode.Skip:
                    if (File.Exists(outputPath))
                    {
                        return false;
                    }
                    break;
                case OverwriteMode.Backup:
                    var hasBackup = await DoBackupFileIfExists(outputPath);
                    if (hasBackup)
                    {
                        break;
                    }
                    return false;
            }

            if (string.IsNullOrWhiteSpace(_targetReferenceSystem))
            {
                return await Clip(layer, outputPath);
            }

            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            var tempOutputPath = Path.Combine(tempDir, Path.GetFileName(outputPath));

            if (await Clip(layer, tempOutputPath))
            {
                if (await Project(tempOutputPath, outputPath))
                {
                    Directory.Delete(tempDir, true);
                    return true;
                }
            }

            return false;

        }
    }
}
