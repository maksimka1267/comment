var onloadCallback = function () {
    grecaptcha.render('dvCaptcha', {
        'sitekey': '6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI',  // Замените на ваш реальный sitekey
        'callback': function (response) {
            $("#hfCaptcha").val(response);
            $("#rfvCaptcha").hide();
        }
    });
};
document.addEventListener("DOMContentLoaded", function () {
    const commentFormContainer = document.getElementById("comment-form-container");
    const showCommentFormBtn = document.getElementById("show-comment-form");

    showCommentFormBtn.addEventListener("click", function () {
        commentFormContainer.classList.toggle("hidden");
        commentFormContainer.scrollIntoView({ behavior: 'smooth' });
    });

    document.querySelectorAll(".reply-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            let commentId = this.getAttribute("data-id");
            let replyFormContainer = document.getElementById("reply-form-" + commentId);

            if (!replyFormContainer.innerHTML) {
                replyFormContainer.innerHTML = `
                            <h5>Ответ на комментарий</h5>
                            <form method="post" action="/Comment/Reply" enctype="multipart/form-data">
                                <input type="hidden" name="ParentId" value="${commentId}">
                                <div class="mb-3">
                                    <label for="userName" class="form-label">Имя</label>
                                    <input type="text" class="form-control" name="UserName" required>
                                </div>
                                <div class="mb-3">
                                    <label for="email" class="form-label">Email</label>
                                    <input type="email" class="form-control" name="Email" required>
                                </div>
                                <div class="mb-3">
                                    <label for="text" class="form-label">Комментарий</label>
                                    <textarea class="form-control" name="Text" required></textarea>
                                </div>
                                <div class="mb-3">
                                    <label for="files" class="form-label">Прикрепить файлы</label>
                                    <input type="file" class="form-control" name="Files" id="files" multiple>
                                </div>
                                <button type="submit" class="btn btn-sm btn-success">Ответить</button>
                            </form>`;
            }

            replyFormContainer.classList.toggle("hidden");
        });
    });

    // Обработчик отправки основного комментария
    document.getElementById("comment-form").addEventListener("submit", async function (event) {
        event.preventDefault();

        let formData = new FormData(this);
        let files = formData.getAll("Files");
        const allowedTypes = ["image/jpeg", "image/png", "image/gif", "text/plain"];
        let hasValidFiles = false;

        // Проверяем файлы на допустимые типы
        for (let file of files) {
            if (file.size > 0 && allowedTypes.includes(file.type)) {
                hasValidFiles = true;
                break;
            }
        }

        // Определяем, какой метод использовать
        let actionUrl = hasValidFiles ? "/Comment/AddWithFile" : "/Comment/Add";
        this.action = actionUrl;

        let response = await fetch(actionUrl, {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            location.reload(); // Обновляем страницу
            window.scrollTo({ top: 0, behavior: 'smooth' }); // Прокручиваем страницу наверх
        } else {
            alert(`Ошибка при добавлении комментария! Статус: ${response.status}`);
        }
    });

    // Обработчик отправки ответа на комментарий
    document.addEventListener("submit", async function (event) {
        if (event.target.closest("form") && event.target.closest("form").action.includes("/Comment/Reply")) {
            event.preventDefault();

            let formData = new FormData(event.target);
            let files = formData.getAll("Files");
            const allowedTypes = ["image/jpeg", "image/png", "image/gif", "text/plain"];
            let hasValidFiles = false;

            `` // Проверка файлов
            for (let file of files) {
                if (file.size > 0 && allowedTypes.includes(file.type)) {
                    hasValidFiles = true;
                    break;
                }
            }

            let actionUrl = hasValidFiles ? "/Comment/ReplyWithFile" : "/Comment/Reply";
            event.target.action = actionUrl;

            let response = await fetch(actionUrl, {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                location.reload(); // Обновляем страницу
                window.scrollTo({ top: 0, behavior: 'smooth' }); // Прокручиваем страницу наверх
            } else {
                alert(`Ошибка при добавлении ответа! Статус: ${response.status}`);
            }
        }
    });
});
const socket = new WebSocket("ws://localhost:7099/ws");  // Указываем URL WebSocket сервера

socket.onopen = () => {
    console.log("WebSocket подключен.");
};

socket.onmessage = (event) => {
    const message = event.data;
    console.log("Получено сообщение: " + message);

    // Показываем уведомление о новом комментарии
    const notificationElement = document.createElement('div');
    notificationElement.classList.add('notification');
    notificationElement.textContent = message;

    // Выводим уведомление на страницу
    document.body.appendChild(notificationElement);

    // Можно сделать стилизацию для уведомлений
    setTimeout(() => {
        notificationElement.remove(); // Удаляем уведомление через 5 секунд
    }, 5000);
};

socket.onerror = (error) => {
    console.log("Ошибка WebSocket: ", error);
};

socket.onclose = () => {
    console.log("WebSocket закрыт.");
};
