# SharpFtpC2 (PoC)

![Banner Image](Assets/Images/banner.png)

SharpFtpC2 is a small, experimental project aimed at exploring the possibility of using FTP(S) for relaying commands and responses between two remote computers. It employs the FTP protocol as a makeshift tunnel through which the computers, both acting as clients connected to an FTP server, can communicate. A simple session management scheme is used to keep track of the exchange of requests and responses.

SharpFtpC2 employs a basic session management system. Although quite elementary, it serves the purpose of keeping the communications synchronized and related, which is essential for the back-and-forth between the remote systems.

It's worth noting that this project can be effortlessly ported by utilizing version control systems such as git, svn, or similar protocols.

⚠️ Please note that SharpFtpC2 is very much in the early stages of development. It lacks critical security features like data encryption to protect transmitted data integrity and confidentiality. Therefore, it's not at all meant for production use or any environment where security is a concern. It's more of a curiosity, a stepping stone, or a learning tool for those intrigued by network communication.

If you have an interest in the nitty-gritty of network communication, or just want to fiddle with C# and .NET Core, SharpFtpC2 might be an intriguing starting point. Don't expect a polished gem, but maybe, just maybe, you might learn something interesting from tinkering with it.

---

## The Story Behing The Project

SharpFtpC2 was born out of the desire to contribute to the [Unprotect Project](https://unprotect.it), particularly its [Network Evasion](https://unprotect.it/category/network-evasion/) category. 

This idea of using FTP as a "tunnel" has roots that run deep. In fact, it brings back fond memories from around 2005 when I was still getting my feet wet in the programming world. Back then, I crossed paths with a remarkably creative French individual who went by the moniker **BlasterWar**. He had conceived a project named **BlasterX**, which, despite being lost to time, was rather avant-garde for its era. 

BlasterWar's ingenuity in his project was to provide an alternative to the conventional reverse connection, where the agent needed to establish a connection back to the controlling or hacking device.

Instead, BlasterWar opted to use FTP (File Transfer Protocol) as the alternative medium and constructed a comprehensive Remote Access Tool around it. The Tool included features such as Screen Capture, Keylogging, and System Management, all transmitted through the FTP tunnel. At the time, FTP was widely popular and a plethora of websites offered free FTP servers to the public. This made it an ideal alternative to reverse or direct connections, which involved port forwarding. Moreover, it provided an added layer of obfuscation for the command and control (C2) as the IP address of the hacker's machine wasn't directly exposed.

Today, utilizing FTP as a tunnel is not a novel concept, as a handful of Command and Control (C2) frameworks have embraced this protocol. However, employing FTP in this manner is fraught with risks. Notably, FTP's transmission of credentials in plain text over the network, combined with the necessity for both parties to possess these credentials, makes it susceptible to a myriad of attacks. Although FTP servers have made strides in addressing these security issues by increasingly adopting FTPS, which integrates SSL/TLS encryption, this adaptation has not been a panacea for all the inherent risks.

With a touch of ingenuity and by drawing inspiration from existing protocols, it is feasible to tackle a substantial number of the existing risks. The current version of SharpFtpC2, however, does not incorporate these mitigations, and that is why it is labeled as experimental for the time being.

## Give a try

To compile this project, you require two components: [Visual Studio](https://visualstudio.microsoft.com/?WT.mc_id=SEC-MVP-5005282) and a dependency for the controller named [CommandLineUtils](https://www.nuget.org/packages/Microsoft.Extensions.CommandLineUtils?WT.mc_id=SEC-MVP-5005282).

*As this project utilizes .NET Core, it can be compiled for various platforms with ease, without necessitating any code modifications. However, you may need to implement specific features tailored to the target platform.*

To begin testing this project swiftly, I recommend employing Docker with the [stilliard/pure-ftpd](https://hub.docker.com/r/stilliard/pure-ftpd/) image. This image supports a range of options, enabling you to rapidly set up your own FTP server with ease.

`docker pull stilliard/pure-ftpd`

### Without TLS

`docker run -d --name ftpd_server -p 21:21 -p 30000-30009:30000-30009 -e "PUBLICHOST: 127.0.0.1" -e "ADDED_FLAGS=-E -A -X -x" -e FTP_USER_NAME=dark -e FTP_USER_PASS=toor -e FTP_USER_HOME=/home/dark stilliard/pure-ftpd`

### With TLS (Recommended)

`docker run -d --name ftpd_server -p 21:21 -p 30000-30009:30000-30009 -e "PUBLICHOST: 127.0.0.1" -e "ADDED_FLAGS=-E -A -X -x --tls=2" -e FTP_USER_NAME=dark -e FTP_USER_PASS=toor -e FTP_USER_HOME=/home/dark -e "TLS_CN=localhost" -e "TLS_ORG=maislaf" -e "TLS_C=FR" stilliard/pure-ftpd`


Feel free to tailor the settings according to your requirements. However, I strongly advise against exposing this test FTP server to local or public networks. It would be more prudent to limit the exposure of this container solely to your host machine.

The `ADDED_FLAGS` option allows you to fine-tune the pure-ftpd server. Explanations for all the flags can be found [here](https://linux.die.net/man/8/pure-ftpd).

Certain flags may necessitate modifications to the functioning of the C2 protocol. For instance, if you employ the `-K` option to retain all files, the ability to delete files via FTP will be disabled. Since the current C2 protocol utilizes this feature, you might need to contemplate alternative approaches, such as file renaming or moving.

## Supported Commands

* Run a shell command and echo response.
* Terminate agent process.

## TODO

- Implement data encryption to ensure the integrity and confidentiality of the request and response communications between the controller and the agent.
- Add more comments to explain the code.
- Demonstrate how to implement a file transfer using actual protocol.
- Command line argument.




