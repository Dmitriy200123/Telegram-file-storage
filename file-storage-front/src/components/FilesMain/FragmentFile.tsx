import React, {memo, useState} from 'react';
import "./FilesMain.scss"
import {ExpandingObject, ModalContent, Rights, TypeFile} from "../../models/File";
import {Link} from 'react-router-dom';
import {OutsideAlerter} from "../utils/OutSideAlerter/OutSideAlerter";
import {ReactComponent as Edit} from "./../../assets/edit.svg";
import {ReactComponent as Download} from "./../../assets/download_2.svg";
import {ReactComponent as Delete} from "./../../assets/delete.svg";
import {useDispatch} from "react-redux";
import {filesSlice} from "../../redux/filesSlice";
import {Dispatch} from "@reduxjs/toolkit";
import {fetchDownloadLink} from "../../redux/thunks/fileThunks";

const {openModal, setOpenFile} = filesSlice.actions

const FragmentFile: React.FC<PropsType> = ({file, rights, types}) => {
    const {fileId, fileName, uploadDate, fileType, sender, chat} = file;
    const dispatch = useDispatch();
    return <React.Fragment key={fileId}>
        <Link className={"files__item files__item_name"} to={`/file/${fileId}`} replace onClick={() => {
            dispatch(setOpenFile(file));
        }}>{fileName}</Link>
        <div className={"files__item"}>{uploadDate}</div>
        <div className={"files__item"}>{types && types[fileType]}</div>
        <div className={"files__item"}>{sender.fullName}</div>
        <div className={"files__item files__item_relative"}>{chat?.name} <Controls rights={rights} id={fileId} fileType={fileType}
                                                                                   dispatch={dispatch}/></div>
    </React.Fragment>
};


const Controls = memo(({id, dispatch, rights, fileType}: { id: string, dispatch: Dispatch<any>, rights: Rights[], fileType:string }) => {
    const [isOpen, changeIsOpen] = useState(false);
    return <OutsideAlerter onOutsideClick={() => changeIsOpen(false)}>
        <div className={"file-controls"}>
            <button onClick={(e) => {
                e.preventDefault();
                changeIsOpen(true);
            }} className={"file-controls__btn"}>
                <div className={"file-controls__circle"}/>
            </button>
            {isOpen && <section className={"file-controls__modal"}>
                {rights.includes(Rights["Переименовывать файлы"]) &&
                <div className={"file-controls__modal-item"}
                     onClick={() => dispatch(openModal({id, content: ModalContent.Edit}))}>
                    <Edit/><span>Переименовать</span></div>}
                {+fileType !== 4 && +fileType !== 5 && <div className={"file-controls__modal-item"} onClick={() => dispatch(fetchDownloadLink(id))}>
                    <Download/><span>Скачать</span></div>}
                {rights.includes(Rights["Удалять файлы"]) &&
                <div className={"file-controls__modal-item file-controls__modal-item_delete"}
                     onClick={() => dispatch(openModal({id, content: ModalContent.Remove}))}>
                    <Delete/><span>Удалить</span></div>
                }
            </section>
            }
        </div>
    </OutsideAlerter>
});

type PropsType = { file: TypeFile, rights: Rights[], types: undefined | ExpandingObject<string> };

export default FragmentFile;


