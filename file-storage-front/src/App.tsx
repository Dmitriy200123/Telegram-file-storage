import React, {FC, useEffect} from 'react';
import './App.css';
import './App.scss';
import {BrowserRouter, Redirect, Route, Switch} from "react-router-dom";
import {Provider, useDispatch} from "react-redux";
import {setupStore} from "./redux/redux-store";
import {OpenedFile} from "./components/FilesMain/File/OpenFile";
import FilesMain from "./components/FilesMain/FilesMain";
import {useAppDispatch, useAppSelector} from "./utils/hooks/reduxHooks";
import {LoadFileMain} from "./components/LoadFile/LoadFileMain";
import {modalContents} from "./components/utils/Modal/Modal";
import {StartPage} from "./components/StartPage/StartPage";
import {Messages} from "./components/utils/Messages/Messages";
import {Navbar} from "./components/Navbar/Navbar";
import Loading from "./components/utils/Loading/Loading";
import {fetchIsAuth, fetchLogout} from "./redux/thunks/profileThunks";
import {ReactComponent as Logout} from "./assets/logout.svg";
import {RightsManagerPanel} from "./components/RightsManagerPanel/RightsManagerPanel";
import {fetchRightsCurrentUser, fetchUserCurrent} from "./redux/thunks/rightsThunks";
import {Rights} from "./models/File";
import {Profile} from "./components/Profile/Profile";

const App: FC = () => {
    const dispatch = useDispatch();
    const {profile} = useAppSelector((state) => state);
    const {messages, loading} = profile;
    useEffect(() => {
        dispatch(fetchIsAuth());
    }, [])
    return (<div className="App app">
        {!!messages.length && <Messages messages={messages} className={"app__messages"}/>}
        {profile.isAuth ? <Main/> : loading ? "Загрузка..." : <StartPage/>}
        {/*<Main/>*/}
    </div>)
}

const Main: FC = () => {
    const dispatch = useAppDispatch();
    useEffect(() => {
        dispatch(fetchUserCurrent());
        dispatch(fetchRightsCurrentUser());
    }, [])
    localStorage.setItem("flag", "false");
    const {filesReducer, profile} = useAppSelector((state) => state);
    const {rights, hasTelegram} = profile;
    const {loading, modalConfirm} = filesReducer;
    const {isOpen, id, content} = modalConfirm;
    const Content = modalContents[content || 0];
    return (<>
        <Navbar className={"app__navbar"}/>
        {loading && <Loading/>}
        <div className={"app__content"}>
            <header className="header">
                <button className={"header__logout"} onClick={() => dispatch(fetchLogout())}><span>Выйти</span>
                    <Logout/></button>
            </header>
            <div className={"app__content-components"}
                 style={{flex: "1 1 auto", display: "flex", flexDirection: "column"}}>
                <Switch>
                    <Route path={"/Profile"} component={Profile}/>
                    {/*<Route path={"/login"} component={StartPage}/>*/}
                    {hasTelegram && <>
                        <Route path={"/files"} exact component={FilesMain}/>
                        <Route path={"/file/:id"} component={OpenedFile}/>
                        {rights?.includes(Rights["Редактировать права пользователей"]) &&
                        <Route path={"/admin"} component={RightsManagerPanel}/>}
                        {rights?.includes(Rights["Загружать файлы"]) &&
                        <Route exact={true} path={"/load/"} component={LoadFileMain}/>}

                        <Redirect to={"/files"}/>
                    </>}
                </Switch>
            </div>
            {isOpen && id && <Content id={id}/>}
        </div>
    </>)
}


const store = setupStore();

function FileStorageApp() {
    return (
        <BrowserRouter>
            <Provider store={store}>
                <App/>
            </Provider>
        </BrowserRouter>
    );
}

export default FileStorageApp;
