using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace RevitPipeConnectionChecker
{
    /// <summary>
    /// Identifies unconnected pipes in the current view and highlights them in red.
    /// Provides a comprehensive report of connection status.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PipeConnectionCheckerCommand : IExternalCommand
    {
        // Colors for visual identification
        private static readonly Color ConnectedColor = new Color(180, 180, 180);   // Gray
        private static readonly Color UnconnectedColor = new Color(250, 0, 0);     // Red

        public Result Execute(ExternalCommandData commandData,ref string message,ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                // Collect all pipes
                List<Element> allPipes = new FilteredElementCollector(doc)
                    .OfClass(typeof(Pipe))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .ToList();

                if (allPipes.Count == 0)
                {
                    TaskDialog.Show("No Pipes",
                        "No pipes found in the current project.");
                    return Result.Cancelled;
                }

                // Identify unconnected pipes
                List<Element> unconnectedPipes = allPipes
                    .Where(p => HasUnusedConnectors(p as Pipe))
                    .ToList();

                //Apply visual overrides
                using (Transaction tx = new Transaction(doc, "Highlight Unconnected Pipes"))
                {
                    tx.Start();

                    View activeView = uiDoc.ActiveView;

                    // Isolate pipes for focus
                    activeView.IsolateElementsTemporary(
                        allPipes.Select(p => p.Id).ToList());

                    // Set hidden line display for clarity
                    activeView.DisplayStyle = DisplayStyle.HLR;

                    // Color connected pipes gray
                    foreach (Element pipe in allPipes)
                    {
                        ApplyColor(activeView, pipe.Id, ConnectedColor);
                    }

                    // Color unconnected pipes red
                    foreach (Element pipe in unconnectedPipes)
                    {
                        ApplyColor(activeView, pipe.Id, UnconnectedColor);
                    }

                    tx.Commit();
                }

                //Show detailed report
                ShowReport(allPipes.Count, unconnectedPipes.Count);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private bool HasUnusedConnectors(Pipe pipe)
        {
            if (pipe?.ConnectorManager == null) return false;
            return pipe.ConnectorManager.UnusedConnectors.Size > 0;
        }

        private void ApplyColor(View view, ElementId elementId, Color color)
        {
            OverrideGraphicSettings settings = view.GetElementOverrides(elementId);
            settings = settings.SetProjectionLineColor(color);
            view.SetElementOverrides(elementId, settings);
        }

        private void ShowReport(int total, int unconnected)
        {
            int connected = total - unconnected;
            double percentageConnected = (double)connected / total * 100;

            TaskDialog dialog = new TaskDialog("Pipe Connection Report")
            {
                MainInstruction = "Analysis Complete",
                MainContent =
                    $" Total Pipes: {total}\n" +
                    $" Connected: {connected} ({percentageConnected:F1}%)\n" +
                    $" Unconnected: {unconnected}\n\n" +
                    (unconnected > 0
                        ? " Unconnected pipes are highlighted in RED in the current view."
                        : " All pipes are properly connected!")
            };

            dialog.Show();
        }
    }
}