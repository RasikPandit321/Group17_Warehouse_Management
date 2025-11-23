using Microsoft.VisualStudio.TestTools.UnitTesting;

// FIX: Disable parallelization because our tests rely on shared file resources (alarm.txt).
// This ensures tests run one at a time and don't lock files from each other.
[assembly: DoNotParallelize]