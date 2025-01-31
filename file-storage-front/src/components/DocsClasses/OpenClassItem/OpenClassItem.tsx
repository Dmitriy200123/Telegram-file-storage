import React, {FC, memo, useEffect, useState} from 'react';
import {Link} from "react-router-dom";
import {ReactComponent as Edit} from "../../../assets/edit.svg";
import classes from "./OpenClassItem.module.scss";
import {Button} from "../../utils/Button/Button";
import {ReactComponent as Delete} from "../../../assets/delete.svg";
import {classesDocsSlice} from "../../../redux/classesDocs/classesDocsSlice";
import {useAppDispatch, useAppSelector} from "../../../utils/hooks/reduxHooks";
import Tag, {CreateTag} from "./Tag/Tag";
import {ClassificationType} from "../../../models/Classification";
import {
    fetchClassification, fetchDeleteClassification,
    fetchDeleteToClassificationWord,
    postAddToClassificationWord
} from "../../../redux/classesDocs/classesDocsThunks";
import {profileSlice} from "../../../redux/profileSlice";
import {MessageTypeEnum, Rights} from "../../../models/File";

const {openModal} = classesDocsSlice.actions
const {addMessage} = profileSlice.actions;

interface match<Params extends { [K in keyof Params]?: string } = {}> {
    params: Params;
    isExact: boolean;
    path: string;
    url: string;
}

type PropsType = { match: match<{ id: string }> }

const OpenClassItemContainer: React.FC<PropsType> = memo(({match}) => {
    const id = match.params["id"];
    const dispatch = useAppDispatch();
    const classifications = useAppSelector(state => state.classesDocs.classifications);
    const [classification, setClassification] = useState<ClassificationType | null>(null);
    useEffect(() => {
        if (classifications === null || classifications.length === 0) {
            dispatch(fetchClassification(id));
            return;
        }
        const found = classifications.find(e => e.id === id);
        if (found) {
            setClassification(found);
        } else {
            dispatch(fetchClassification(id));
            //todo: req
        }
    }, [id, classifications])


    function openRename() {
        dispatch(openModal({type: "edit", args: {id: id, name: classification?.name || ""}}));
    }


    return (
        <OpenClassItem openRename={openRename} classification={classification} rights={[Rights["Удаление классификаций"]]}/>
    );
});

type PropsTypeOpen = { openRename: () => void, classification: ClassificationType | null, rights?: Rights[] }


const OpenClassItem: FC<PropsTypeOpen> = memo(({openRename, classification, }) => {
    const {rights} = useAppSelector((state) => state.profile);
    const dispatch = useAppDispatch();
    if (!classification)
        return <>Не найдено</>;

    function createTag(value: string) {
        if (!classification)
            return;
        if (classification.classificationWords?.find((c) => c.value === value)){
            return dispatch(addMessage({type: MessageTypeEnum.Error, value: "Такое слово уже есть"}));
        }
        
        dispatch(postAddToClassificationWord({classId: classification?.id || "", value}))
    }

    function deleteClassification() {
        if (classification)
            dispatch(openModal({type: "remove", args: {id: classification.id}}));
    }

    const tags = classification.classificationWords?.map(c => {
        function removeTag() {
            // @ts-ignore
            dispatch(fetchDeleteToClassificationWord({classId: classification.id, tagId: c.id}))
        }

        return <Tag key={c.id} tag={c} removeTag={removeTag}/>
    });

    return <div className={classes.classItem}>
        <div className={classes.classItem__header}>
            <h2 className={classes.classItem__title}>Классификации документов</h2>
            <Link className={classes.classItem__close} to={"/docs-сlasses"}/>
        </div>
        <div className={classes.classItem__content}>
            {rights?.includes(Rights["Редактирование классификаций"]) && <h3 onClick={openRename} className={classes.classItem__contentTitle}>{classification.name} {<Edit/>}</h3>}
            {rights?.includes(Rights["Редактирование классификаций"]) && <div className={classes.classItem__tags}>
                {tags}
                <CreateTag setTag={createTag}/>
            </div>}
            {rights?.includes(Rights["Удаление классификаций"]) &&
            <div className={classes.classItem__btns}>
                <Button onClick={deleteClassification}
                        type={"danger"} className={classes.classItem__btn_delete}><span>Удалить</span><Delete/></Button>
            </div>}
        </div>
    </div>;
});

export default OpenClassItemContainer;





