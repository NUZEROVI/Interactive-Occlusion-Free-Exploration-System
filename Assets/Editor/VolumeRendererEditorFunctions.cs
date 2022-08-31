using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityVolumeRendering
{
    public class VolumeRendererEditorFunctions
    {
        [MenuItem("Volume Rendering/Load dataset/Load raw dataset")]
        static void ShowDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                RAWDatasetImporterEditorWindow wnd = (RAWDatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(RAWDatasetImporterEditorWindow));
                if (wnd != null)
                    wnd.Close();

                wnd = new RAWDatasetImporterEditorWindow(file);
                wnd.Show();
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load DICOM")]
        static void ShowDICOMImporter()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                bool recursive = true;

                // Read all files
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                if (!fileCandidates.Any())
                {
#if UNITY_EDITOR
                    if (UnityEditor.EditorUtility.DisplayDialog("Could not find any DICOM files",
                        $"Failed to find any files with DICOM file extension.{Environment.NewLine}Do you want to include files without DICOM file extension?", "Yes", "No"))
                    {
                        fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    }
#endif
                }

                if (fileCandidates.Any())
                {
                    IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
                    IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);
                    float numVolumesCreated = 0;

                    foreach (IImageSequenceSeries series in seriesList)
                    {
                        VolumeDataset dataset = importer.ImportSeries(series);
                        if (dataset != null)
                        {
                            if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                            {
                                if (EditorUtility.DisplayDialog("Optional DownScaling",
                                    $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                                {
                                    dataset.DownScaleData();
                                }
                            }

                            VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                            obj.transform.position = new Vector3(numVolumesCreated, 0, 0);
                            numVolumesCreated++;
                        }
                    }
                }    
                else
                    Debug.LogError("Could not find any DICOM files to import.");

                
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load PARCHG dataset")]
        static void ShowParDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                VolumeDataset dataset = importer.Import(file);

                if (dataset != null)
                {
                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                }
                else
                {
                    Debug.LogError("Failed to import datset");
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load image sequence")]
        static void ShowSequenceImporter()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            
            if (Directory.Exists(dir))
            {
                List<string> filePaths = Directory.GetFiles(dir).ToList();
                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.ImageSequence);

                IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(filePaths);

                foreach(IImageSequenceSeries series in seriesList)
                {
                    VolumeDataset dataset = importer.ImportSeries(series);
                    if (dataset != null)
                    {
                        if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                        {
                            if (EditorUtility.DisplayDialog("Optional DownScaling",
                                $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                            {
                                dataset.DownScaleData();
                            }
                        }
                        VolumeObjectFactory.CreateObject(dataset);
                    }
                }
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }
   

        [MenuItem("Volume Rendering/1D Transfer Function")]
        public static void Show1DTFWindow()
        {
            TransferFunctionEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/2D Transfer Function")]
        public static void Show2DTFWindow()
        {
            TransferFunction2DEditorWindow.ShowWindow();
        }
    
    }
}
