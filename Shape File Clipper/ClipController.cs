using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace Geomo.Util
{
    enum OverwriteMode
    {
        Overwrite, Backup, Skip
    }

    class ClipController
    {
        private readonly string _clipExtentShapeFile;
        private readonly string _outputDirectory;
        private readonly string _postfix;
        private readonly string _backupFolderName;
        private readonly OverwriteMode _overwriteMode;
        private readonly CancelableProgressorSource _cancelHandler;

        public ClipController(string clipExtentShapeFile, string outputDirectory, string postfix, string backupFolderName,
            OverwriteMode overwriteMode, CancelableProgressorSource cancelHandler)
        {
            this._clipExtentShapeFile = clipExtentShapeFile;
            this._outputDirectory = outputDirectory;
            this._postfix = postfix;
            this._backupFolderName = backupFolderName;
            this._overwriteMode = overwriteMode;
            this._cancelHandler = cancelHandler;
        }

        private async Task<bool> CopyShapeFile(string shapeFile, string targetPath)
        {

            var copyToolParams
                = await QueuedTask.Run(() => Geoprocessing.MakeValueArray(shapeFile, targetPath));

            IGPResult result = await Geoprocessing.ExecuteToolAsync("management.CopyFeatures", copyToolParams, null,
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

        private async Task<bool> ClipShapeFile(string shapeFile, string outputPath)
        {

            var clipToolParams
                = await QueuedTask.Run(() => Geoprocessing.MakeValueArray(shapeFile, _clipExtentShapeFile, outputPath));

            IGPResult result = await Geoprocessing.ExecuteToolAsync("analysis.Clip", clipToolParams, null,
                _cancelHandler.Progressor, GPExecuteToolFlags.None | GPExecuteToolFlags.GPThread);

            return !result.IsFailed;

        }

        public async Task<bool> ProcessShapeFile(string shapeFile)
        {

            var shapeFileName = $"{Path.GetFileNameWithoutExtension(shapeFile)}_{_postfix}.shp";
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

            return await ClipShapeFile(shapeFile, outputPath);

        }
    }
}
