# Youtube-Transcriber
Youtube Transcriber is a powerful web application for converting YouTube videos to text transcriptions, designed with a focus on accuracy and user experience. It features a responsive interface built with React.js and Material-UI, allowing users to easily submit YouTube URLs and receive accurate text transcriptions powered by Google's Speech-to-Text API. The backend, powered by ASP.NET Core and MongoDB, ensures efficient data handling with Redis caching for optimized performance and reduced API calls. The application leverages Redis not only for caching transcription results but also for managing user sessions and temporary storage of processing status, significantly improving response times. YTranscriber offers seamless video processing through a distributed architecture, utilizing RabbitMQ for background tasks and AWS Simple Email Service SES for email notifications, while FFmpeg handles audio extraction to ensure high-quality transcription results.

# Documentation
All the necessary information about RabbitMQ, Redis, AWS SES, and Google Text To Speech API can be found at the links:
https://www.rabbitmq.com/tutorials,
https://redis.io/docs/latest/,
https://docs.aws.amazon.com/ses/latest/dg/send-email.html,
https://cloud.google.com/text-to-speech/docs/basics

# Footage
![register](https://github.com/user-attachments/assets/6714e7fa-bd9e-4d9e-a900-3964e8311d30)
![ses](https://github.com/user-attachments/assets/be304594-70fe-46d8-b94c-9d136e486a78)
![login](https://github.com/user-attachments/assets/b2fa72b7-e404-4c0f-acfd-d15976981de4)
![process](https://github.com/user-attachments/assets/ee4b22e7-f035-4908-b7dd-87d1bada2717)
![result](https://github.com/user-attachments/assets/39a5b5d6-588c-4a9a-9305-ce43b161d2be)
![history](https://github.com/user-attachments/assets/166f5b50-0393-4a85-b5e3-78156e8025d0)


