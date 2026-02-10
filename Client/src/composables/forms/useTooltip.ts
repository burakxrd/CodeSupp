import { ref } from 'vue';

interface TooltipState {
    visible: boolean;
    data: any;
    x: number;
    y: number;
    placement: 'top' | 'bottom';
}

export function useTooltip() {
    const tooltip = ref<TooltipState>({ 
        visible: false, 
        data: null, 
        x: 0, 
        y: 0, 
        placement: 'top' 
    });

    const showTooltip = (event: Event | MouseEvent, data: any) => {
        const target = event.target as HTMLElement;
        if (!target) return;

        const rect = target.getBoundingClientRect();
        const tooltipHeight = 200; 
        const placeBelow = rect.top < tooltipHeight + 20; 

        tooltip.value = {
            visible: true,
            data: data,
            x: rect.left + (rect.width / 2) + window.scrollX, 
            y: placeBelow ? (rect.bottom + 10 + window.scrollY) : (rect.top - 10 + window.scrollY),
            placement: placeBelow ? 'bottom' : 'top'
        };
    };

    const hideTooltip = () => { 
        tooltip.value.visible = false; 
    };

    return {
        tooltip,
        showTooltip,
        hideTooltip
    };
}