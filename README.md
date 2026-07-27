# Revit Pipe Connection Checker

A Revit API plugin that scans all pipes in a model, isolates them in the active view, and highlights unconnected pipes in red — turning a manual QA pass into a one-click check.

## Features

- Scans every pipe in the active Revit model using `FilteredElementCollector`
- Detects unconnected pipe ends by checking each pipe's `ConnectorManager` for unused connectors
- Temporarily isolates all pipes in the current view for focused review
- Applies color overrides: connected pipes shown in gray, unconnected pipes highlighted in red
- Displays a summary report with total pipe count, connected count, unconnected count, and connection percentage

## How it works

1. Collects all `Pipe` elements in the active document
2. Checks each pipe for unused connectors to identify unconnected ends
3. Opens a transaction and applies `OverrideGraphicSettings` to color-code pipes by connection status
4. Shows a `TaskDialog` report summarizing the results

## Tech Stack

- C#
- Revit API
- .NET

## Screenshots

See the `screenshots` folder for the plugin in action — the highlighted view and the connection report dialog.

## Author

Esraa Omran — AEC Software Developer | BIM Developer
[LinkedIn](https://www.linkedin.com/in/esraa-omran/) · [Portfolio](https://esraaomran0.github.io/)