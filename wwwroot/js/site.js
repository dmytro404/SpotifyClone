class Base64 {
    static #textEncoder = new TextEncoder();
    static #textDecoder = new TextDecoder();

    // https://datatracker.ietf.org/doc/html/rfc4648#section-4
    static encode = (str) => btoa(String.fromCharCode(...Base64.#textEncoder.encode(str)));
    static decode = (str) => Base64.#textDecoder.decode(Uint8Array.from(atob(str), c => c.charCodeAt(0)));

    // https://datatracker.ietf.org/doc/html/rfc4648#section-5
    static encodeUrl = (str) => this.encode(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    static decodeUrl = (str) => this.decode(str.replace(/\-/g, '+').replace(/\_/g, '/'));

    static jwtEncodeBody = (header, payload) => this.encodeUrl(JSON.stringify(header)) + '.' + this.encodeUrl(JSON.stringify(payload));
    static jwtDecodePayload = (jwt) => JSON.parse(this.decodeUrl(jwt.split('.')[1]));
}

document.addEventListener('submit', e => {
    const form = e.target;

    if (form.id == 'auth-form') {
        e.preventDefault();
        const formData = new FormData(form);
        const login = formData.get("user-login");
        const password = formData.get("user-password");

        // https://datatracker.ietf.org/doc/html/rfc7617#section-2
        const userPass = `${login}:${password}`;
        const credentials = Base64.encode(userPass);
        console.log(login, password, credentials);
        fetch("/api/user", {
            method: 'GET',
            headers: {
                'Authorization': `Basic ${credentials}`
            }
        }).then(r => r.json())
            .then(j => {
                if (j.status.isOk) {
                    window.location.reload();
                }
            })
            .catch(console.error);
    }

    if (form.id == "admin-group-form") {
        e.preventDefault();

        form.querySelectorAll(".text-danger").forEach(span => span.textContent = "");

        fetch("/api/group", {
            method: 'POST',
            body: new FormData(form)
        })
            .then(r => r.json().then(j => ({ status: r.status, body: j })))
            .then(({ status, body }) => {
                if (status === 200) {
                    alert(body.status);
                    form.reset();
                } else if (status === 400) {
                    const errors = body.errors;
                    for (const field in errors) {
                        const errorSpan = document.getElementById(`${field}-error`);
                        if (errorSpan) {
                            errorSpan.textContent = errors[field][0];
                        }
                    }
                } else {
                    alert("Помилка на сервері");
                }
            })
            .catch(console.error);
    }


    if (form.id == "admin-product-form") {
        e.preventDefault();
        form.querySelectorAll(".text-danger").forEach(span => span.textContent = "");

        fetch("/api/product", {
            method: 'POST',
            body: new FormData(form)
        })
            .then(r => r.json().then(j => ({ status: r.status, body: j })))
            .then(({ status, body }) => {
                if (status === 200) {
                    alert(body.data.name);
                    form.reset();
                } else if (status === 400) {
                    const errors = body.errors;
                    for (const field in errors) {
                        const errorSpan = document.getElementById(`${field}-error`);
                        if (errorSpan) {
                            errorSpan.textContent = errors[field][0];
                        }
                    }
                } else {
                    alert("Помилка на сервері");
                }
            })
            .catch(console.error);
    }
});
