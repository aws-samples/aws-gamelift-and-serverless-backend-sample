FROM ubuntu:18.04
RUN apt-get update && apt-get upgrade -y
RUN apt-get install ca-certificates -y
COPY . /app
CMD /app/BotClient.x86_64