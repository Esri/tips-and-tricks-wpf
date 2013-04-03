tips-and-tricks-wpf
===================

Examples, demos, tips and tricks, and best practice code shown during the ArcGIS Runtime SDK for WPF Tech Sessions and Demo Theater at the Esri Dev Summit 2013. 
These projects show you how to accomplish mapping both online and offline, editing and analysis with the ArcGIS Runtime SDK for WPF.
The samples frequently use ArcGIS Online basemaps and services. Learn more [here](http://www.arcgis.com/about/).


## Contents of Repo

* The repo contains 3 folders which represent the 3 session topics presented at the Esri Dev Summit:
	* Best-Development-Practices-and-Patterns
	* Developing-Mapping-Applications
	* Implementing-Analysis-Editing-and-Offline-Applications

* Session folders contain one folder per individual demo. Each demo folder contains two or more sub folders:
	* App:- 			The compilation output location (release & debug)
	* Maps-and-Data:- 	Packaged maps and data used by the demonstration
	* SourceCode:-		A Visual Studio 2012 Project containing the source code for the demo
	* SourceData:-		Any source ArcMap documents, file geodatabases, geoprocessing toolboxes, etc which have been used in the creation of the Packages used in the code.


## Features

* \Best-Development-Practices-and-Patterns
	* \DynamicLayers
		* Using the DynamicLayers capability of the RuntimeLocalServer to add Shapefiles and raster datasets to the map. Also demonstrates making HTTP requests directly to the RuntimeLocalServer.
	* \Editing-Stand-Alone-Tables
		* Editing standalone geodatabase tables via the FeatureLayer class. Also shows explicitly raising and subsequently handling the Initialized and UpdateCompleted events.
	* \Editing-Without-Any-UI
		* Editing geodatabase feature classes programmatically via the FeatureLayer class. Also shows handling the Initialized and UpdateCompleted events.
	* \Efficiently-Adding-Graphics
		* Adding graphics to the graphics layer in the most efficient pattern.
	* \Graphics-To-From-Json
		* Using the JSON format to persist graphics and geometries.
	* \Persisting-Schema-On-Gp-Outputs
		* Demonstrates specifying the schema and an attribute value for Geoprocessing input parameters which will be persisted on the output(s).
	* \PictureMarkerSymbol-From-Wpf
		* Programmatically creating WPF UI elements and forcing an offscreen render in order to create images to be used as PictureMarkerSymbols for Graphics.

* \Developing-Mapping-Applications
	* \Adding-Layers
		* Setting up the Map and adding basemaps and operational layers which reference online services.
	* \Adding-Graphics
		* Creating Graphics with geometry, attributes and renderers and efficiently adding them to the map.
	* \Opening-WebMaps
		* Opening WebMaps from ArcGIS Online by WebMap ID and enabling the hardware accelerated display. 
	* \World-Geocoding-and-Routing
		* Finding places and getting directions using the Esri World Geocoding Service and the Network Analysis service for North America.
	* \WebMap-Identity-Manager
		* Using secure content from ArcGIS Online or an on-premise Portal for ArcGIS instance using the IdentityManager and SignIn dialog.
		
* \Implementing-Analysis-Editing-and-Offline-Applications
	* \Adding-Layers-LocalTiledLayer
		* Working with Tile Packages and exploded tile caches.
	* \Adding-Layers-Offline
		* Adding basemaps and operational layers which reference content for supporting offline workflows.
	* \Editing-Online
		* Editing features within online FeatureServices using the Toolkit assembly UI components.
	* \Editing-Offline
		* Editing features within LocalFeatureService instances using the Toolkit assembly UI components.
	* \Trace-Geometric-Network
		* Using geoprocessing to perform a network trace on a geometric network.


## Instructions


1. Fork and then clone the repo. 
2. Run and try the samples.


## Requirements

* ArcGIS Runtime SDK for WPF 10.1.1
* Visual Studio 2010, Visual Studio 2012 or Expression Blend.
* .NET Framewmork 4.0 or 4.5


## Resources


* [ArcGIS Runtime SDK for WPF Resource Center](http://resources.arcgis.com/en/communities/runtime-wpf/index.html)
* [ArcGIS Blog](http://blogs.esri.com/esri/arcgis/)
* [twitter@esri](http://twitter.com/esri)


## Issues


Find a bug or want to request a new feature?  Please let us know by submitting an issue.


## Contributing


Anyone and everyone is welcome to contribute. 


## Licensing
Copyright 2012 Esri


Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at


   http://www.apache.org/licenses/LICENSE-2.0


Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.


A copy of the license is available in the repository's license.txt file.


[](Esri Tags: Devsummt 2013 ArcGIS-Runtime-SDK-for-WPF)
[](Esri Language: WPF)​