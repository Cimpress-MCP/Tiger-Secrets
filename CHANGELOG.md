### What's new in 2.1.0 (Released 2020-09-08)

* The library has begun tracking the AWS SDK 3.5 line.
* The library has been configured for nullability.

### What's new in 2.0.1 (Released 2020-01-13)

* Misconfigured options are now handled by bailing out gracefully.

### What's new in 2.0.0 (Released 2019-11-05)

* The IDs which the library will check are no longer configured by convention.
* The configuration now accepts a collection of secret IDs (probably ARNs) from which to read.
  * It no longer accepts a "base ID".

### What's new in 1.1.0 (Released 2019-04-29)

* The ability to wait for an in-progress secrets refresh to complete before ending execution has been added.
  * This is likely only to be meaningful to a Lambda Function.

### What's new in 1.0.0 (Released 2019-02-20)

* Everything is new!
