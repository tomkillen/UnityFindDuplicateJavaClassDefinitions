# Unity Find Duplicate Java Class Definitions

A utility class for Unity 3D to discover duplicate class definitions in included aar and jar files.

Have you ever spent a couple of hours tearing your hair out trying to find the source of a Unity android build error such as 

> com.android.build.api.transform.TransformException: java.util.zip.ZipException: duplicate entry: x/y/z/Some.class See the Console for details.

or 

> CommandInvokationFailure: Gradle build failed. 
> FAILURE: Build failed with an exception.
> * What went wrong:
> Execution failed for task ':transformClassesWithJarMergingForRelease'.
> com.android.build.api.transform.TransformException: java.util.zip.ZipException: duplicate entry: x/y/z/Some.class


It can be a tedious process to discover exactly which JAR or AAR files contain the conflicting entries.

## Well worry no more

This utility will scan your project for JAR's and AAR's, inspect what Java definitions they contain, and identify potential conflicts.

## What it does

Add's a menu option (Assets -> Android -> Scan for duplicate class definitions) that will scan your project to discover all JAR and AAR files, determine which definitions each contains, and identify potential conflicting or duplicated definitions.

## What it does not do

It only scans Java class libraries (e.g. JAR and AAR files) that are in your project. It does not explore your gradle files to discover any conflicts that can occur from types imported at that stage. It is limited to files actually present in your Unity project directory.

It also does not check to see if a class library is actually being used. It will simply identify *potential* sources of conflict and alert you to them for further inspection.

# Installation

Drop the file into your Unity 3D project.

# How to use

## Easy Mode

From the toolbar, choose Assets -> Android -> Scan for duplicate class definitions.

Any duplicate definitions will be output as warnings in the editor console.

Note: depending on the size of your project, it may take some time to scan each file. If you have a small project this might take only moments. If you have a large project, go make a coffee.

## Extended Mode

You can call `FindDuplicateJavaDefinitions.Scan("/path/to/some/dir")` to get a list of all class definitions and conflicts found under that directory and it's sub-directories. Useful if you want to automate this process, e.g. with Jenkins.

# Supported Platforms

Currently only Windows and Mac OS X are supported. It should be simple to add Linux support, being essentially identical to Mac but I don't have a Linux machine to test that out on. HMU.

Developed and tested using Unity 2017.4.12f1

# Dependencies

You will need to have the JDK installed. This script executes a shell command (e.g. "jar tf xyz.jar") to inspect what each file contains. 

You need the JDK installed to make builds for Android from Unity anyway so I'll leave that problem up to you, but you can start here https://docs.unity3d.com/Manual/android-sdksetup.html


