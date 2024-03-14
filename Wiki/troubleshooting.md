# Wiki

* [Commandline](commandline.md)
* [Programming-Guidelines](programming-guidelines.md)
* [REST communication between Optimization and Webapp](rest.md)
* [Testing-Guidelines](testing.md)
* [Troubleshooting](troubleshooting.md)
* [gitlab runner](gitlab-runner.md)
* [Home](home.md)

## Troubleshooting

Below we'll list known issues when setting up the project or using it and their solutions.

### EmguCv throws System.DllNotFoundException or System.BadImageFormatException (concerning "cvextern.dll")

Problems might be solvable following the steps described in: http://www.emgu.com/wiki/index.php/Download_And_Installation#Windows

However, there might simply be a lot of dependency of cvextern.dll missing. It is as of yet unclear how to handle this case properly.

### Tests throw System.BadImageFormatException

The project is targeted at x64 architectures. Default for tests in VisualStudio is typically AnyCPU or x86. You'll need to change the default test architecture:

Test -> Test Settings -> Default Processor Architecture -> X64

### RegionMarker

If you experience weird behavior with RegionMarker, check if you use Halcon 13.0 or higher. The 12.x version is known to cause some issues.


### Test Pipelines are pending eternally or stuck

The gitlab runner might be down. Login on deeplearning2 and restart it:
`sudo systemctl start gitlab-runner.service`

### Testrunner (NUnit) hangs on tests

If the test runner starts running the tests but gets stuck silently (i.e. no exceptions are thrown), this might be due to licensing issues with halcon. "Feature has expired" (and possibly "BadImageFormatExceptions") thrown by Halcon are just not passed along properly.

A helpful commanline option for the NUnit Runner (nunit3-console.exe) is `--timeout=1000` which skips the stuck tests and actually prints the halcon exceptions.

Check if:
* The halcon license is still valid
* The docker container uses the host network
    - if you try to debug manually: docker run -it --network host <image_id>
    - if you configured a new runner: make sure that the gitlab config uses the docker option for host networks

