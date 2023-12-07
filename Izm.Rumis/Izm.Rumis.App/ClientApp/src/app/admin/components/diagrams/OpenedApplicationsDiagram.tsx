'use client'

import { Typography } from "antd";
import { Legend, Pie, PieChart, ResponsiveContainer } from "recharts"

const {Title} = Typography

const OpenedApplicationsDiagram = () => {
    const data = [
        { name: 'Iesniegts', value: 15, fill: '#de425b' },
        { name: 'Atlikta izskatīšana', value: 8, fill: '#f3babc' },
        { name: 'Piešķirts resurs', value: 3, fill: '#bad0af'},
        { name: 'Sagatavots izsniegšanai', value: 12, fill: '#488f31' },
    ];
    return (
        <div className="h-[500px]">
            <Title level={4}>Kopā atvērtie pieteikumi</Title>
            <ResponsiveContainer width="100%" height="100%">
                <PieChart width={500} height={430}>
                    <Pie 
                        data={data} 
                        dataKey="value" 
                        cx="50%" 
                        cy="50%" 
                        fill="#8884d8" 
                        label
                    />
                    <Legend />
                </PieChart>
            </ResponsiveContainer>
        </div>
    )
}

export default OpenedApplicationsDiagram