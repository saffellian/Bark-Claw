Thank you for purchasing Sensor Toolkit!

The documentation is available online at: http://www.micosmo.com/sensortoolkit
And there is an example scene under the Examples directory.

If you have any questions, feature requests or if you have found a bug then please send me an email at micosmogames@gmail.com

ChangeLog

1.1.3:
- Greatly reduced garbage generated.
- Sensor DetectedObjects and DetectedObjectsOrderedByDistance changed to IEnumerable types, so they can be enumerated without allocations.

1.2.0
- Sensors can now be configured with tag filters.
- Added 'GetVisibleTransforms' and 'GetVisiblePositions' methods to trigger/range sensors, to query the raycast targets on a detected object that are in line of sight.
- range/trigger sensors can be configured to only detect objects with LOSTargets components.
- Added new PlayMaker actions.

1.2.1
- Fixed SensorGetVisibleRaycastTargets playmaker action so that visible Transforms are returned correctly.
- Removed duplicate TagSelectorPropertyDrawer.cs editor file which was causing builds to fail.

1.2.2
- Small bug fix for tag filters

1.2.3
- Fixed issue where RangeSensor wasn't firing DetectionLost events

1.3.0
- Added SteeringRig and SteeringRig2D, along with a few example steering prefabs
- Added 3 new example scenes: Action, Stealth and Space
- Minor bug fixes

1.3.1
- Added LOSTargets to Stealth example
- Minor fix to Physics layers in examples
- Made steering rig gizmos bigger

1.3.2
- FOVCollider and FOVCollider2D now expose their mesh through the FOVMesh property so it can be rendered.
- Added RenderFOVCollider and RenderFOVCollider2D for easily rendering fov colliders
- Fix for occasional error occuring with trigger sensor when gameobjects are rapidly activated and deactivated

1.3.3
- TriggerSensor2D must have a rigid body with sleep mode never sleep.
- New playmaker actions for controlling steering rig.

1.3.4
- Fix bug causing null reference exception

1.3.5
- Added a copy of SpaceDemo with 10x scale to demonstrate how to configure its sensors at a larger scale.
- Fixed a bug causing detection events to not fire when LOS testing was enabled.
- A fix to several SensorGetDetected actions with a bug ocurring when 'Store Component' field is used.

1.3.6
- Fixed a bug involving the SteeringRig2D Playmaker actions

1.4.0
- RaySensor and RaySensor2D now have a radius parameter, for casting lines with thickness
- A new playmaker action for setting the IgnoreList of a sensor
- A new playmaker action for testing if a gameobject is detected by a sensor

1.5.0
- Minimum Unity version now 5.3.8f2
- Ray sensors and Range Sensor now use non-allocating versions of physics functions
- New attributes on ray and range sensors for configuring initial buffer sizes
- Hunted down all remaining lines of code which allocate garbage. Sensor toolkit should no longer generate any garbage
- Various bugfixes and improvements

1.5.1
- Bugfixes

1.6.0
- Sensor events now pass the sensor instance which caused the event to the handler function.
- Fixed a bug causing RaySensor events not to fire.

1.6.1
- Fixed a bug causing LostDetection events to fire late on the TriggerSensor
- Fixed a bug where an exception is thrown when accessing DetectedObjects from a Sensor Event on the TriggerSensor

1.6.2
- Removed GUI layers from cameras in demo scenes, as theyre deprecated in newer versions of Unity.
- Fixed a bug affecting FOV colliders when an Undo is performed.

1.6.3
- Fix bug where ray sensors wouldnt reset when tested in edit mode