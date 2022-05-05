import React, {ChangeEvent, FC, useEffect, useState} from 'react';
import {InputText} from "../utils/Inputs/Text/InputText";
import {Button} from "../utils/Button/Button";
import classes from "./DocsClasses.module.scss";
import ClassesItems from "./Inner/ClassesItems/DocsClasses";
import DocsClassesModal from "./Modals/DocsClassesModal/DocsClassesModal";
import {useAppDispatch, useAppSelector} from "../../utils/hooks/reduxHooks";
import {classesDocsSlice} from "../../redux/classesDocs/classesDocsSlice";
import {Route, Switch} from "react-router-dom";
import OpenClassItem from "./OpenClassItem/OpenClassItem";
import {fetchClassifications, fetchCountClassifications} from "../../redux/classesDocs/classesDocsThunks";
import Paginator from "../utils/Paginator/Paginator";
import {ReactComponent as PlusIcon} from "./../../assets/plus.svg";

type PropsType = {}

const {openModal, closeModal, setIsFetchClassifications} = classesDocsSlice.actions
const DocsClassesPage: FC<PropsType> = () => {
    const {type, isOpen, args} = useAppSelector((state) => state.classesDocs.modal);
    const dispatch = useAppDispatch();
    return (<>
            <Switch>
                <Route path={"/docs-сlasses"} component={DocsClasses} exact={true}/>
                <Route path={"/docs-сlasses/:id"} component={OpenClassItem}/>
            </Switch>
            {isOpen &&
            <DocsClassesModal onOutsideClick={() => dispatch(closeModal())} modalType={type || "create"} args={args}/>}
        </>
    );
};

const DocsClasses: FC<PropsType> = () => {
    const dispatch = useAppDispatch();
    const classifications = useAppSelector(state => state.classesDocs.classifications);
    const count = useAppSelector(state => state.classesDocs.count);
    const isFetchClassifications = useAppSelector(state => state.classesDocs.fetchClassifications);
    const [filters, setFilters] = useState({page: 1, take: 10, query: undefined as undefined | string});

    function fetchClasses() {
        dispatch(fetchCountClassifications(filters.query));
        dispatch(fetchClassifications({
            skip: filters.take * (filters.page - 1),
            take: filters.take,
            query: filters.query
        }));
    }

    useEffect(() => {
        fetchClasses();
    }, [filters]);

    useEffect(() => {
        if (!isFetchClassifications) {
            return
        }

        dispatch(setIsFetchClassifications(false));
        if (classifications && classifications.length !== 0) {
            return fetchClasses();
        }

        if (filters.page > 1)
            setFilters((prev) => ({...prev, page: prev.page - 1}))
    }, [isFetchClassifications])

    function onChangeInput(e: ChangeEvent<HTMLInputElement>) {
        setFilters((prev) => ({...prev, page: 1, query: e.target.value}));
    }

    function onChangePage(page: number) {
        setFilters((prev) => ({...prev, page: page}));
    }

    return (
        <div className={classes.block}>
            <h2 className={classes.h2}>Классификации документов</h2>
            <div className={classes.background}>
                <div className={classes.content}>
                    <div className={classes.controls}>
                        <InputText className={classes.controlInput} onChange={onChangeInput}
                                   placeholder={"Поиск классификации документов"}/>
                        <Button onClick={() => dispatch(openModal({type: "create"}))} className={classes.controlBtn}>
                            <PlusIcon className={classes.icon}/>Создать классификацию
                        </Button>
                    </div>
                    {classifications && classifications.length > 0 ? <ClassesItems classifications={classifications}/> :
                        <Empty notFound={(filters.query?.length || 0) > 0}/>}
                </div>
                <Paginator pageHandler={onChangePage} current={filters.page} count={Math.ceil(count / filters.take)}/>
            </div>

        </div>
    );
};

const Empty:FC<{notFound:boolean}> = ({notFound}) => {
    return <div className={classes.classesEmpty}>Классификации документов {!notFound ? "пока не созданы" : "не найдены"}</div>;
}


export default DocsClassesPage;