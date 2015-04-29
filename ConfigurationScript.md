# Configuration Script Example #

This is an example for a typical configuration script. Of course, the passwords are all fake.

```
AccountAlias 12030000 12345678 "Andreas DKB"
AccountAlias 12030000 "4907********1234" "Andreas DKB Visa"

# Collector plugins

# Get my credit card statements
Dkb 12345678 12345

# Get statements of my checking account
Hbci MyCheckingAccount 12345
```

The first parameter of the Hbci collector is the account ID for the HBCI module. Configure the HBCI access details by clicking "HBCI Setup" in the settings dialog.