# Heartbleed-Challenge Kryptographie

Die Grundidee dieser Lösung ist, dass man einen Kandidaten für `d` dadurch überprüfen kann, indem man eine mit `e` und `N` verschlüsselte Nachricht (`m`) wieder entschlüsselt. Kommt das richtige Ergebnis raus, ist `d` das inverse zu `e`.

Letztendlich müssen dann nur für jede Position innerhalb der Datei alle Schlüssellängen durchprobiert werden. Hier bietet sich eine Optimierung an:
1. `d` wird nicht größer als `phi(N)` sein. Wir kennen `phi(N)` zwar nicht, können stattdessen aber `N` als Obergrenze nehmen, was die Schlüssellängenbereich noch einmal einschränkt.
2. Wir können mit den größeren Schlüsselkandidaten anfangen, denn kleine Schlüssel sind unwahrscheinlich.

Im Ordner `python` bzw. `c-sharp` sind entsprechende Implementierungen dieses Algorithmus. Die Python-Lösung soll die Lösung möglichst einfach implementieren, die C#-Version dafür effizient.

## Vergleich
Zur Vergleichbarkeit werden die beiden Lösung in einem Docker-Container ausgeführt, welcher intern die Ausführungszeit misst.

```bash
cd c-sharp
docker build -t krypto-c-sharp .
docker run --rm krypto-c-sharp
# ...
# Ausgabe:
Found d at 886:128:
99927539702245916006383313519731656077828344258465375843888986008441295110874393258182025248720868292157071412441117118926508755556467311797191399024923918816071461009813484236652211423670107073264735642406057828993776773259251528156154234594052633158521143237899740605674695654985213105621487297133889842467
real	0m 1.73s
user	0m 13.09s
sys	0m 0.01s
```

```bash
cd python
docker build -t krypto-python .
docker run --rm krypto-python
# ...
# Ausgabe:
Number of bits of N: 1024
Current candidate length: 128
Found d at 886:128:
99927539702245916006383313519731656077828344258465375843888986008441295110874393258182025248720868292157071412441117118926508755556467311797191399024923918816071461009813484236652211423670107073264735642406057828993776773259251528156154234594052633158521143237899740605674695654985213105621487297133889842467
real	0m 3.13s
user	0m 3.12s
sys	0m 0.00s
```

Was man sieht: `real` ist die tatsächlich benötigte Zeit (Wallclock-Time). Wenn man `user` und `system`addiert, erhält man die Zeit, die benötigt gewesen wäre, wenn man nur einen Prozessorkern zur Verfügung gehabt hätte. Ist sie identisch mit `real`, ist dies ein Zeichen dafür, dass nur ein Kern verwendet wurde.

Bei meinem Testlaut (i7 mit 4 Kernen bzw. 8 Threads) liegt die `user + system`-Zahl deutlich höher in der C#-Version als in der Python-Version. Dies lässt sich dadurch erklären, dass 8 Kerne gleichzeitig verschiedene Positionen für `d` ausprobieren. Dadurch wir das Ergebnis früher gefunden, es waren aber mehr Kerne beteiligt. Lässt man die C#-Lösung mit einem Thread laufen, ist diese tatsächlich langsamer als die Python-Lösung (~7 Sekunden Wallclock-Time). Dies liegt vermutlich daran, dass die ModPow-Implementierung von C# offenbar um einiges langsamer ist.
