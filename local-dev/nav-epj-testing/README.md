# nav-epj-testing

Klientregistreringen appen trenger for å kunne startes fra [`navikt/nav-epj`](https://github.com/navikt/nav-epj) lokalt.

- `forer-legeerklaering-client.yaml` — limes inn i `smart.clients` i nav-epj sin `src/main/resources/application-local.yaml`.

Hele oppsettet og testscenarioene står i [docs/LOKALT-MED-NAV-EPJ.md](../../docs/LOKALT-MED-NAV-EPJ.md).

Patchen og seed-skriptet som lå her før, er fjernet. De var skrevet mot nav-epj slik den var i august 2026, og gjelder ikke lenger.
