# KerbalCraft
Addon for Kerbal Space Program to easily share your inventions.

## Server

The server is a REST service written for nodejs which uses MongoDB for data storage.
It also contains a parser for the file format the craft files are saved as (ConfigNode). When new craft are uploaded the server will use the parser to automatically add more information about the ship to the database.

## Ingame KSP addon

The addon provides an ingame GUI which can access the information on the server. The window shows a list of the most recent uploads and can load them directly in the editor. Uploading and downloading ships only requires a single click.

## Current State

The current state is functional but limited in a few aspects:
- There is no form of authentication implemented. That means everyone can list, add and even delete everything without restriction.
- There is no detection for non-stock parts implemented. That means that a ship can contain parts provided by addons which will cause an error when someone tries to load it without having the proper addons installed. At least KSP generates a nice error message explaining why it could not load the ship and lists the missing parts.
- There is no filtering implemented for the list yet. The ships will always be sorted by date with the most recent uploads at the front.
