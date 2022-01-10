import "./StartPage.scss";
import Logo from "./../../assets/logos/grey.png";
import {Button} from "../utils/Button/Button";
import {FC, memo, useEffect, useState} from "react";
import {GitlabAuth} from "gitlab-auth";
import {useHistory} from "react-router-dom";
import {useAppDispatch} from "../../utils/hooks/reduxHooks";
import {fetchAuthGitlab} from "../../redux/thunks/profileThunks";

export const StartPage: FC = memo(() => {
    const dispatch = useAppDispatch();
    const [clicked, ChangeClicked] = useState(localStorage.getItem("flag") === "true");
    const changeClicked = (flag: boolean) => {
        localStorage.setItem("flag", String(flag));
        ChangeClicked(flag);
    }
    useEffect(() => {
        localStorage.setItem("flag", "false");
        let dataAuthGit = localStorage.getItem("oidc.user:https://git.66bit.ru:392b8f8766b8da0f5f64edaa50b89b633d302ab0fd7f94aa482d5510e1a97cda");
        if (!dataAuthGit)
            dataAuthGit = sessionStorage.getItem("oidc.user:https://git.66bit.ru:392b8f8766b8da0f5f64edaa50b89b633d302ab0fd7f94aa482d5510e1a97cda");

        if (dataAuthGit) {
            const json = JSON.parse(dataAuthGit);
            dispatch(fetchAuthGitlab(json.access_token));
        }
    }, [localStorage, sessionStorage])
    return (
        <div>
            {clicked && <GitlabAuth
                host="https://git.66bit.ru"
                application_id="392b8f8766b8da0f5f64edaa50b89b633d302ab0fd7f94aa482d5510e1a97cda"
                redirect_uri={process.env.REACT_APP_REDIRECT_URL as string}
                scope="api openid profile email"
                secret={"3446700f6a9e418bee18bbd32f4df0fc6e2749182faaf3bebbbc3ebd4c5b1325"}
            />
            }
            <div className={"start-page"}>
                <div>
                    <img src={Logo}/>
                </div>
                <div className={"start-page__content"}>
                    <h1 className={"start-page__title"}>Хранилище файлов</h1>
                    <p className={"start-page__description"}>— Сервис, позволяющий автоматически собирать файлы из чатов
                        Telegram и сохранять в хранилище с информацией о файле.</p>
                    <Button type={"white"} className={"start-page__btn"} onClick={() => changeClicked(true)}>Войти через
                        GitLab →</Button>
                </div>
            </div>
        </div>

    );
})




