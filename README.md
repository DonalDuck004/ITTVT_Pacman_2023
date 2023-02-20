# ITTVT_Pacman_2023
![ITTVT](https://raw.githubusercontent.com/DonalDuck004/ITTVT_Pacman_2023/master/PacManWPF/Assets/Images/CreditiDellaScuolaCheMiHaFattoDeprimere.png)
Questo proggetto è stato creato in occasione dell'open day 2023 della scuola ITTVT(Istituto Tecnico Tecnlogico di Viterbo)
Il sito della scuola è accessibile presso questo link [ittvt.edu.it](https://www.ittvt.edu.it/)

# Dipendenze necessarie per la versione pre-compilata
1. NET 7.0 scariabile presso il [sito ufficiale](https://dotnet.microsoft.com/en-us/download/dotnet/7.0), in caso non sia installato, verrà richiesta la sua installazione da una finestra che si aprirà al posto del gioco


# Descrizione Rapida
Il progetto è composta da 6 sotto-proggetti:
Proggetti da installare/compilare nel client:
1. GitUpdateChecker - Non necessario per giocare, si occupa di controllare se sono presenti [nuovi rilasci pre-compilati ](https://github.com/DonalDuck004/ITTVT_Pacman_2023/releases). Può essere integrato in PacManWPF insieme a UpdateInstaller
2. PacmanOnlineMapsWPF - Non necessario per giocare, permette la navigazione e lo scaricamento dei mondi caricati da altri utenti. Può essere integrato in PacManWPF
3. PacmanWPF - Necessario per giocare, in quanto è il gioco stesso lol
4. UpdateInstaller - Non necessario per giocare, me neccessario se si usa GitUpdateChecker. Questo progetto si occuppa di lanciare il setup di installazione aggiornato
5. WorldsBuilderWPF - Non necessario per giocare, ma necessario per creare mondi giocabili

Proggetti da installare/compilare nel server:
1. World Backend - Server scritto in Python a cui PacmanOnlineMapsWPF si collega per effettuare richieste

# Note
Ogni versione pre-compilata disponibile nella sezione rilasci, ha il setup di installazione realizzato con [WinRar](https://winrar.it/).\
Non è stato utilizzato alcun tipo di engine grafico, per facilitare la lettura del codice agli studenti con un livello di reparazione nella media.
World Backend necessita di un server PostgresSQL a cui collegarsi, oltre ad un interprete Python 3.7+
Successivamente sarà indicata come root(\) la certella in è situtato il file .sln
Il termine "cartella di compilazione" indicherà la cartella, la cartella in cui sarà disponibile il risultato della compilazione
*Quindi ad esempio la cartella di compilazione di PacManWPF, è \PacManWPF\PacManWPF\bin\Release\net7.0\ se si sta compilando una Release, altirmenti in \PacManWPF\PacManWPF\bin\Debug\net7.0\ se si sta compilando in modalità Debug*


# Configurazione del Backend(World Backend)
Per impostare i parametri di connessione al database è necessario modifcare il file dbhandler.py situtato nella cartella "modules". Questi parametri di default sono impostati su:
```
    USER = "postgres"
    PASSWORD = "D0D04W4sH3re"
    PORT = 5432
    HOST = "192.168.1.65"
    DB_NAME = "ittvt_pacman"
```
Inoltre va rimpizzato l'url alla [seguente riga](https://github.com/DonalDuck004/ITTVT_Pacman_2023/blob/master/WorldsBuilderWPF/MainWindow.xaml.cs#L702) di \WorldsBuilderWPF\MainWindow.xaml.cs:
```
            var a = client.PostAsync("http://localhost:8000/add_world", content).Result;
```
Infine va rimpizzato l'url alla [seguente riga](https://github.com/DonalDuck004/ITTVT_Pacman_2023/blob/master/PacmanOnlineMapsWPF/PacmanOnlineMaps.xaml.cs#L38) di \PacmanOnlineMapsWPF\PacmanOnlineMaps.xaml.cs
```
public static string HOST = "http://localhost:8000/";
```
*Questa parte del documento è in fase di revisione*

# Integrazione di PacmanOnlineMapsWPF con PacmanWPF
Una volta compilato PacmanOnlineMapsWPF è necessario prelevare il dll, e posizionarlo nella cartella di compilazione di PacmanWPF\

# Integrazione di GitUpdateChecker e UpdateInstaller con PacmanWPF
Seguire i passaggi per l'integrazione di PacmanOnlineMapsWPF ma con GitUpdateChecker
## Integrazione di UpdateInstaller
1. Compilare il proggetto UpdateInstaller
2. Recarsi nella cartella di compilazione, quindi copiarne il conenuto
3. Creare una cartella denominata "Updater" nella cartella di compilazione di PacmanWPF
4. Incollare in essa i file precedentemente copiati

# Problemi/Bisogno di aiuto?
Creare aprire un problema nella sezione "Issues" di questa repo
